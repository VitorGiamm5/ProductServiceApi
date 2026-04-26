using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;
using System.Data.Common;

namespace ProductServiceApp.Infrastructure.Database.Interceptors;

public class ResilienceInterceptor : DbCommandInterceptor
{
    private readonly ResiliencePipeline _pipeline;

    public ResilienceInterceptor()
    {
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 10,
                Delay = TimeSpan.FromSeconds(3),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<NpgsqlException>(ex => IsTransient(ex))
                    .Handle<TimeoutException>(),
                OnRetry = args =>
                {
                    Console.WriteLine(
                        "[Resiliência] Banco indisponível — tentativa {Attempt}/{Max}. " +
                        "Próxima em {Delay:s}s. Erro: {Error}",
                        args.AttemptNumber + 1,
                        10,
                        args.RetryDelay,
                        args.Outcome.Exception?.Message);

                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(30))
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