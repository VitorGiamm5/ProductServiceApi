using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;
using System.Data.Common;

namespace ProductServiceApp.Infrastructure.Database.Interceptors;

public class ResilienceInterceptor : DbCommandInterceptor
{
    private const int MaxRetryAttempts = 10;
    private const int SecondsToTimeout = 30;
    private const int DelaySeconds = 3;
    private readonly ILogger<ResilienceInterceptor> _logger;
    private readonly ResiliencePipeline _pipeline;

    public ResilienceInterceptor(ILogger<ResilienceInterceptor> logger)
    {
        _logger = logger;

        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = MaxRetryAttempts,
                Delay = TimeSpan.FromSeconds(DelaySeconds),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<NpgsqlException>(ex => IsTransient(ex))
                    .Handle<TimeoutException>(),
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        args.Outcome.Exception,
                        "[Resiliencia] Banco indisponivel. Tentativa {Attempt}/{Max}. Proxima em {Delay}.",
                        args.AttemptNumber + 1,
                        MaxRetryAttempts,
                        args.RetryDelay);

                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(SecondsToTimeout))
            .Build();
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        await _pipeline.ExecuteAsync(
            async ct => await base.ReaderExecutingAsync(command, eventData, result, ct),
            cancellationToken);

        return result;
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        await _pipeline.ExecuteAsync(
            async ct => await base.ScalarExecutingAsync(command, eventData, result, ct),
            cancellationToken);

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await _pipeline.ExecuteAsync(
            async ct => await base.NonQueryExecutingAsync(command, eventData, result, ct),
            cancellationToken);

        return result;
    }

    private static bool IsTransient(NpgsqlException ex) => ex.IsTransient || ex.SqlState is
        "08000" or
        "08003" or
        "08006" or
        "08001" or
        "08004" or
        "57P01" or
        "57P02" or
        "57P03";
}
