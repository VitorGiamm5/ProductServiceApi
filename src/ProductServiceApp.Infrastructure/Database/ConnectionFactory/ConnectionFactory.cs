using Microsoft.Extensions.Configuration;
using Npgsql;
using Polly;
using Polly.Retry;

namespace ProductServiceApp.Infrastructure.Database.ConnectionFactory;

public class ConnectionFactory : IConnectionFactory
{
    private readonly string _writeConnectionString;
    private readonly string _readConnectionString;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ConnectionFactory(IConfiguration configuration)
    {
        _writeConnectionString = configuration.GetConnectionString("PostgresWrite")!;
        _readConnectionString = configuration.GetConnectionString("PostgresRead")!;

        _retryPolicy = Policy
            .Handle<NpgsqlException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2s, 4s, 8s
                onRetry: (exception, timeSpan, attempt, _) =>
                {
                    //_logger.LogWarning(exception,
                    //    "Falha na conexão com o banco. Tentativa {Attempt} aguardando {Delay}s",
                    //    attempt, timeSpan.TotalSeconds);
                });
    }

    public NpgsqlConnection CreateWriteConnection() =>
    new(_writeConnectionString);

    public NpgsqlConnection CreateReadConnection() =>
        new(_readConnectionString);

    public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action) =>
        await _retryPolicy.ExecuteAsync(action);

    public async Task ExecuteWithRetryAsync(Func<Task> action) =>
        await _retryPolicy.ExecuteAsync(action);
}
