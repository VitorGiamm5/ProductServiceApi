using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ProductServiceApp.Infrastructure.Database.ConnectionFactory;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Interceptors;

namespace ProductServiceApp.Infrastructure;

public static class SetupInfrastructure
{
    private static readonly int _maxRetryCount = 3;
    private static readonly int _maxRetryDelay = 8;
    private static readonly int _secondsToTimeout = 3;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Write Application
        services.AddSingleton<ResilienceInterceptor>();

        services.AddDbContext<ReadOnlyDbContext>((provider, options) =>
        {
            var connectionString = BuildSafeReadConnectionString(
                configuration.GetConnectionString("PostgresRead")!);

            var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();

            options
                .UseNpgsql(dataSource, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: _maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(_maxRetryDelay),
                        errorCodesToAdd: null);

                    npgsql.CommandTimeout(_secondsToTimeout);
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableDetailedErrors()
                .AddInterceptors(provider.GetRequiredService<ResilienceInterceptor>());
        });

        #endregion

        #region Read Application

        services.AddDbContext<ReadOnlyDbContext>((provider, options) =>
        {
            var connectionString = BuildSafeReadConnectionString(
                configuration.GetConnectionString("PostgresRead")!);

            var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();

            options
                .UseNpgsql(dataSource, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: _maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(_maxRetryDelay),
                        errorCodesToAdd: null);

                    npgsql.CommandTimeout(_secondsToTimeout);
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableDetailedErrors()
                .AddInterceptors(provider.GetRequiredService<ResilienceInterceptor>());
        });

        #endregion

        services.AddSingleton<IConnectionFactory, ConnectionFactory>();

        return services;
    }

    private static string BuildSafeReadConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        // Target Session Attributes só é válido com múltiplos hosts
        // Se tiver apenas um host, remove para não quebrar em ambiente local
        var hasMultipleHosts = builder.Host?.Contains(',') ?? false;

        if (!hasMultipleHosts)
        {
            builder.TargetSessionAttributes = TargetSessionAttributes.Any.ToString();
            builder.LoadBalanceHosts = false;
        }

        return builder.ConnectionString;
    }
}
