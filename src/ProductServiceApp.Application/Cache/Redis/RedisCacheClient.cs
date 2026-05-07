using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductServiceApp.Application.Metrics;
using StackExchange.Redis;

namespace ProductServiceApp.Application.Cache.Redis;

public sealed class RedisCacheClient : IRedisCacheClient, IAsyncDisposable
{
    private readonly RedisCacheOptions _options;
    private readonly ILogger<RedisCacheClient> _logger;
    private readonly IConnectionMultiplexer _readConnection;
    private readonly IConnectionMultiplexer _writeConnection;
    private readonly RedisCircuitBreaker _readCircuitBreaker;
    private readonly RedisCircuitBreaker _writeCircuitBreaker;

    public RedisCacheClient(
        IOptions<RedisCacheOptions> options,
        ILogger<RedisCacheClient> logger)
    {
        _options = options.Value;
        _logger = logger;

        var readConnectionString = string.IsNullOrWhiteSpace(_options.ReadConnectionString)
            ? _options.ConnectionString
            : _options.ReadConnectionString;

        var writeConnectionString = string.IsNullOrWhiteSpace(_options.WriteConnectionString)
            ? _options.ConnectionString
            : _options.WriteConnectionString;

        _readConnection = ConnectionMultiplexer.Connect(ParseConfiguration(readConnectionString));
        _writeConnection = ConnectionMultiplexer.Connect(ParseConfiguration(writeConnectionString));
        _readCircuitBreaker = new RedisCircuitBreaker(_options, logger, "read");
        _writeCircuitBreaker = new RedisCircuitBreaker(_options, logger, "write");
    }

    public Task<string?> GetStringAsync(
        string feature,
        string operation,
        string key,
        CancellationToken cancellationToken)
        => ExecuteAsync(
            _readConnection,
            _readCircuitBreaker,
            feature,
            operation,
            "read",
            key,
            cancellationToken,
            async database => (string?)await database.StringGetAsync(BuildKey(key)));

    public Task SetStringAsync(
        string feature,
        string operation,
        string key,
        string payload,
        RedisCacheEntryOptions options,
        CancellationToken cancellationToken)
    {
        var payloadBytes = System.Text.Encoding.UTF8.GetByteCount(payload);
        AppMetrics.CachePayloadBytes
            .WithLabels(feature, operation)
            .Observe(payloadBytes);

        if (payloadBytes > _options.MaxCachePayloadBytes)
        {
            AppMetrics.CacheWriteSkippedTotal
                .WithLabels(feature, operation, "payload_too_large")
                .Inc();

            _logger.LogWarning(
                "Redis cache write skipped for {CacheFeature}/{CacheOperation}. Payload has {PayloadBytes} bytes and limit is {MaxPayloadBytes}.",
                feature,
                operation,
                payloadBytes,
                _options.MaxCachePayloadBytes);

            return Task.CompletedTask;
        }

        return ExecuteAsync(
            _writeConnection,
            _writeCircuitBreaker,
            feature,
            operation,
            "write",
            key,
            cancellationToken,
            async database =>
            {
                await database.StringSetAsync(
                    BuildKey(key),
                    payload,
                    options.AbsoluteExpirationRelativeToNow ?? options.SlidingExpiration);

                return true;
            });
    }

    public Task RemoveAsync(
        string feature,
        string operation,
        string key,
        CancellationToken cancellationToken)
        => ExecuteAsync(
            _writeConnection,
            _writeCircuitBreaker,
            feature,
            operation,
            "write",
            key,
            cancellationToken,
            async database => await database.KeyDeleteAsync(BuildKey(key)));

    public async ValueTask DisposeAsync()
    {
        await _readConnection.CloseAsync();

        if (!ReferenceEquals(_readConnection, _writeConnection))
            await _writeConnection.CloseAsync();
    }

    private async Task<T?> ExecuteAsync<T>(
        IConnectionMultiplexer connection,
        RedisCircuitBreaker circuitBreaker,
        string feature,
        string operation,
        string dependencyOperation,
        string key,
        CancellationToken cancellationToken,
        Func<IDatabase, Task<T>> executeAsync)
    {
        if (!circuitBreaker.TryEnter())
        {
            AppMetrics.CacheCircuitOpenTotal
                .WithLabels(feature, operation, dependencyOperation)
                .Inc();

            _logger.LogWarning(
                "Redis cache {RedisOperation} skipped for {CacheFeature}/{CacheOperation} because circuit breaker is open.",
                dependencyOperation,
                feature,
                operation);

            return default;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (cancellationToken.IsCancellationRequested)
                return default;

            var database = connection.GetDatabase();
            var result = await executeAsync(database);

            circuitBreaker.ReportSuccess();
            ObserveSuccess(feature, operation, dependencyOperation, result);

            return result;
        }
        catch (Exception ex)
        {
            circuitBreaker.ReportFailure(ex);
            ObserveFailure(feature, operation, dependencyOperation, ex);

            _logger.LogWarning(
                ex,
                "Redis cache {RedisOperation} failed for {CacheFeature}/{CacheOperation} and key {CacheKey}.",
                dependencyOperation,
                feature,
                operation,
                key);

            return default;
        }
        finally
        {
            stopwatch.Stop();
            AppMetrics.CacheOperationDuration
                .WithLabels(feature, operation, dependencyOperation)
                .Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }

    private string BuildKey(string key) => $"{_options.InstanceName}{key}";

    private ConfigurationOptions ParseConfiguration(string connectionString)
    {
        var configuration = ConfigurationOptions.Parse(connectionString);
        var operationTimeoutMilliseconds = Math.Max(1, _options.OperationTimeoutMilliseconds);

        configuration.AbortOnConnectFail = false;
        configuration.AsyncTimeout = Math.Max(configuration.AsyncTimeout, operationTimeoutMilliseconds);
        configuration.SyncTimeout = Math.Max(configuration.SyncTimeout, operationTimeoutMilliseconds);
        configuration.ConnectTimeout = Math.Max(configuration.ConnectTimeout, operationTimeoutMilliseconds);

        return configuration;
    }

    private static void ObserveSuccess<T>(
        string feature,
        string operation,
        string dependencyOperation,
        T? result)
    {
        if (dependencyOperation == "read")
        {
            AppMetrics.CacheReadTotal.WithLabels(feature, operation).Inc();

            if (result is null)
                AppMetrics.CacheMissTotal.WithLabels(feature, operation).Inc();
            else
                AppMetrics.CacheHitTotal.WithLabels(feature, operation).Inc();

            return;
        }

        AppMetrics.CacheWriteTotal.WithLabels(feature, operation).Inc();
    }

    private static void ObserveFailure(
        string feature,
        string operation,
        string dependencyOperation,
        Exception exception)
    {
        var reason = exception switch
        {
            RedisTimeoutException => "timeout",
            OperationCanceledException => "canceled",
            _ => "redis_error"
        };

        if (dependencyOperation == "read")
            AppMetrics.CacheReadErrorTotal.WithLabels(feature, operation, reason).Inc();
        else
            AppMetrics.CacheWriteErrorTotal.WithLabels(feature, operation, reason).Inc();
    }
}
