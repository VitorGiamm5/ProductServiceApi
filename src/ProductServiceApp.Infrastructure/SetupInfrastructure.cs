using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Polly;
using Polly.Retry;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
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

        #region Polly Retry Policy

        int retryCount = configuration.GetSection("RetryPolicy:DelayBetweenRetriesInSeconds").Value is not null
            ? int.Parse(configuration.GetSection("RetryPolicy:DelayBetweenRetriesInSeconds").Value!)
            : _maxRetryDelay;

        int maxRetryCount = configuration.GetSection("RetryPolicy:MaxRetryCount").Value is not null
            ? int.Parse(configuration.GetSection("RetryPolicy:MaxRetryCount").Value!)
            : _maxRetryCount;

        RetryPolicy retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(
                retryCount: maxRetryCount,
                sleepDurationProvider: attempt => 
                    TimeSpan.FromSeconds(Math.Pow(retryCount, attempt)),
                onRetry: (exception, timespan, attempt, context) =>
                {
                    Console.WriteLine($"Retry {attempt} fail with error: {exception.Message}. Lets try again {timespan}.");
                });

        #endregion

        #region Write Context (Primary)

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
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
                        .MigrationsHistoryTable("__EFMigrationsHistory", "dbSchemaGoodHamburger")
                        .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                    );

                var resilienceInterceptor = serviceProvider.GetRequiredService<ResilienceInterceptor>();
                options.AddInterceptors(resilienceInterceptor);
            });
        });

        #endregion

        #region Read Context (Replica)

        services.AddDbContext<ReadOnlyDbContext>((serviceProvider, options) =>
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

        #region Repositories

        AddProductsRepositories(services);

        #endregion

        return services;
    }

    private static void AddProductsRepositories(IServiceCollection services)
    {
        services.TryAddScoped<IProductCommandRepository<ProductEntity>, ProductCommandRepository>();
        services.TryAddScoped<IProductQueryRepository<ProductEntity>, ProductQueryRepository>();
    }
}