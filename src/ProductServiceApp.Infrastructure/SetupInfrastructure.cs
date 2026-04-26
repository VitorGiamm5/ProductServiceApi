using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Polly;
using Polly.Retry;
using ProductServiceApp.Domain.Products.Entities;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Infrastructure.Database.ConnectionFactory;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Interceptors;
using ProductServiceApp.Infrastructure.Database.Repositories.Products.Commands;
using ProductServiceApp.Infrastructure.Database.Repositories.Products.Queries;

namespace ProductServiceApp.Infrastructure;

public static class SetupInfrastructure
{
    private static readonly int _maxRetryCount = 3;
    private static readonly int _maxRetryDelay = 2;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Interceptors

        services.AddSingleton<ResilienceInterceptor>();

        #endregion

        #region Repositories

        AddRepositories(services);

        #endregion

        #region Polly Retry Policy

        RetryPolicy retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(
                retryCount: _maxRetryCount,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(_maxRetryDelay, attempt)),
                onRetry: (exception, timespan, attempt, context) =>
                {
                    Console.WriteLine($"Retry {attempt} fail with error: {exception.Message}. Lets try again {timespan}.");
                });

        #endregion

        #region Write Context (Primary)

        services.AddDbContextPool<ApplicationDbContext>((serviceProvider, options) =>
        {
            retryPolicy.Execute(() =>
            {
                var connectionString = configuration.GetConnectionString("PostgresWrite")
                    ?? throw new InvalidOperationException("Connection string 'PostgresWrite' not found.");

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString)
                {
                    ConnectionStringBuilder =
                    {
                        IncludeErrorDetail = true,
                        Timeout = 100
                    }
                };

                options.UseNpgsql(connectionString);

                options
                    .EnableDetailedErrors()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .UseNpgsql(dataSourceBuilder.Build(), b => b
                        .MigrationsHistoryTable("__EFMigrationsHistory", "dbproducts")
                        .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                    );

                var resilienceInterceptor = serviceProvider.GetRequiredService<ResilienceInterceptor>();
                options.AddInterceptors(resilienceInterceptor);
            });
        });

        #endregion

        #region Read Context (Replica)

        services.AddDbContextPool<ReadOnlyDbContext>((serviceProvider, options) =>
        {
            retryPolicy.Execute(() =>
            {
                var connectionString = configuration.GetConnectionString("PostgresRead")
                    ?? throw new InvalidOperationException("Connection string 'PostgresRead' not found.");

                options.UseNpgsql(connectionString)
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                var resilienceInterceptor = serviceProvider.GetRequiredService<ResilienceInterceptor>();
                options.AddInterceptors(resilienceInterceptor);
            });
        });

        #endregion

        services.AddSingleton<IConnectionFactory, ConnectionFactory>();

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.TryAddScoped<IProductsCommandRepository<ProductEntity>, ProductsCommandRepository>();
        services.TryAddScoped<IProductsQueryRepository<ProductEntity>, ProductsQueryRepository>();
    }
}