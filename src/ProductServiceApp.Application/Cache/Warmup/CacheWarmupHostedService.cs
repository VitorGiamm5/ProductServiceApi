using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductServiceApp.Application.Metrics;

namespace ProductServiceApp.Application.Cache.Warmup;

internal sealed class CacheWarmupHostedService(
    IServiceProvider serviceProvider,
    IOptions<CacheWarmupOptions> options,
    ILogger<CacheWarmupHostedService> logger)
    : BackgroundService
{
    private readonly CacheWarmupOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled || !_options.RunOnStartup)
        {
            logger.LogInformation("Cache warmup skipped because it is disabled.");
            return;
        }

        var warmupTask = ExecuteWarmupAsync(stoppingToken);

        if (_options.BlockStartupUntilComplete)
            await warmupTask;
    }

    private async Task ExecuteWarmupAsync(CancellationToken stoppingToken)
    {
        try
        {
            var startupDelay = TimeSpan.FromSeconds(Math.Max(0, _options.StartupDelaySeconds));
            if (startupDelay > TimeSpan.Zero)
                await Task.Delay(startupDelay, stoppingToken);

            using var timeoutCts = new CancellationTokenSource(
                TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);

            await using var scope = serviceProvider.CreateAsyncScope();
            var features = scope.ServiceProvider
                .GetServices<ICacheWarmupFeature>()
                .Where(IsFeatureEnabled)
                .ToArray();

            if (features.Length == 0)
            {
                logger.LogInformation("Cache warmup found no enabled features.");
                return;
            }

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = linkedCts.Token,
                MaxDegreeOfParallelism = Math.Max(1, _options.MaxDegreeOfParallelism)
            };

            await Parallel.ForEachAsync(features, parallelOptions, async (feature, ct) =>
            {
                await WarmupFeatureAsync(feature, ct);
            });
        }
        catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogWarning("Cache warmup stopped because the configured timeout was reached.");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Cache warmup stopped because the application is shutting down.");
        }
    }

    private bool IsFeatureEnabled(ICacheWarmupFeature feature)
    {
        if (!_options.Features.TryGetValue(feature.FeatureName, out var featureOptions) || !featureOptions.Enabled)
        {
            AppMetrics.CacheWarmupSkippedTotal
                .WithLabels(feature.FeatureName, "disabled")
                .Inc();

            return false;
        }

        return true;
    }

    private async Task WarmupFeatureAsync(ICacheWarmupFeature feature, CancellationToken cancellationToken)
    {
        var featureOptions = _options.Features[feature.FeatureName];
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Starting cache warmup for feature {CacheFeature}.", feature.FeatureName);

            var result = await feature.WarmupAsync(featureOptions, cancellationToken);

            AppMetrics.CacheWarmupSuccessTotal
                .WithLabels(feature.FeatureName)
                .Inc();

            AppMetrics.CacheWarmupItemsTotal
                .WithLabels(feature.FeatureName, "all")
                .Inc(result.ItemsLoaded);

            AppMetrics.CacheWarmupItemsTotal
                .WithLabels(feature.FeatureName, "by_id")
                .Inc(result.ItemsByIdWarmed);

            logger.LogInformation(
                "Cache warmup finished for feature {CacheFeature}. ItemsLoaded={ItemsLoaded}, ItemsByIdWarmed={ItemsByIdWarmed}.",
                feature.FeatureName,
                result.ItemsLoaded,
                result.ItemsByIdWarmed);
        }
        catch (Exception ex)
        {
            AppMetrics.CacheWarmupErrorTotal
                .WithLabels(feature.FeatureName, ex is OperationCanceledException ? "timeout" : "error")
                .Inc();

            logger.LogWarning(ex, "Cache warmup failed for feature {CacheFeature}.", feature.FeatureName);
        }
        finally
        {
            stopwatch.Stop();

            AppMetrics.CacheWarmupDuration
                .WithLabels(feature.FeatureName)
                .Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }
}
