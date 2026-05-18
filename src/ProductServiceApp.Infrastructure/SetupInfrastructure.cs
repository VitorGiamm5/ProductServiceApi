using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Security;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.Infrastructure.Database.Interceptors;
using ProductServiceApp.Infrastructure.Database.Repositories.Orders.Commands;
using ProductServiceApp.Infrastructure.Database.Repositories.Orders.Queries;
using ProductServiceApp.Infrastructure.Database.Repositories.Products.Commands;
using ProductServiceApp.Infrastructure.Database.Repositories.Products.Queries;

namespace ProductServiceApp.Infrastructure;

public static class SetupInfrastructure
{
    private static readonly int _maxRetryAttempts = 3;
    private static readonly int _maxRetryDelay = 2;
    private const string WriteDataSourceKey = "postgres-write";
    private const string ReadDataSourceKey = "postgres-read";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Interceptors

        services.TryAddScoped<ICurrentUserContext, SystemCurrentUserContext>();
        services.AddSingleton<ResilienceInterceptor>();
        services.TryAddKeyedSingleton<NpgsqlDataSource>(WriteDataSourceKey, (_, _) =>
            BuildPostgresDataSource(configuration, "PostgresWrite"));
        services.TryAddKeyedSingleton<NpgsqlDataSource>(ReadDataSourceKey, (_, _) =>
            BuildPostgresDataSource(configuration, "PostgresRead"));

        #endregion

        #region Polly Retry Policy

        var retryPolicySection = configuration.GetSection("RetryPolicy");
        var retryDelaySeconds = GetPositiveInt(retryPolicySection, "DelayBetweenRetriesInSeconds", _maxRetryDelay);
        var maxRetryAttempts = GetPositiveInt(retryPolicySection, "MaxRetryAttempts", _maxRetryAttempts);

        static RetryPolicy CreateRetryPolicy(int retryDelaySeconds, int maxRetryAttempts, ILogger logger) =>
            Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    retryCount: maxRetryAttempts,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromSeconds(Math.Pow(retryDelaySeconds, attempt)),
                    onRetry: (exception, timespan, attempt, context) =>
                    {
                        logger.LogWarning(
                            exception,
                            "Retry {Attempt} failed. Next attempt in {RetryDelay}.",
                            attempt,
                            timespan);
                    });

        #endregion

        #region Write Context (Primary)

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var logger = serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(SetupInfrastructure).FullName!);

            var retryPolicy = CreateRetryPolicy(retryDelaySeconds, maxRetryAttempts, logger);

            retryPolicy.Execute(() =>
            {
                var dataSource = serviceProvider.GetRequiredKeyedService<NpgsqlDataSource>(WriteDataSourceKey);

                options
                    .EnableDetailedErrors()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .UseNpgsql(dataSource, b => b
                        .EnableRetryOnFailure(maxRetryAttempts, TimeSpan.FromSeconds(_maxRetryDelay), null)
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
            var logger = serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(SetupInfrastructure).FullName!);

            var retryPolicy = CreateRetryPolicy(retryDelaySeconds, maxRetryAttempts, logger);

            retryPolicy.Execute(() =>
            {
                var dataSource = serviceProvider.GetRequiredKeyedService<NpgsqlDataSource>(ReadDataSourceKey);

                options.UseNpgsql(dataSource, b =>
                        b.EnableRetryOnFailure(maxRetryAttempts, TimeSpan.FromSeconds(_maxRetryDelay), null))
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                var resilienceInterceptor = serviceProvider.GetRequiredService<ResilienceInterceptor>();
                options.AddInterceptors(resilienceInterceptor);
            });
        });

        #endregion

        #region Repositories

        AddProductsRepositories(services);
        AddOrdersRepositories(services);

        #endregion

        return services;
    }

    #region Products Repositories

    private static void AddProductsRepositories(IServiceCollection services)
    {
        services.TryAddScoped<IProductCommandRepository<ProductEntity>, ProductCommandRepository>();
        services.TryAddScoped<IProductQueryRepository<ProductEntity>, ProductQueryRepository>();
    }

    #endregion

    #region Orders Repositories

    private static void AddOrdersRepositories(IServiceCollection services)
    {
        services.TryAddScoped<IOrderCommandRepository, OrderCommandRepository>();
        services.TryAddScoped<IOrderQueryRepository, OrderQueryRepository>();
        services.TryAddScoped<IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity>, OrderDiscountRuleQueryRepository>();
    }

    #endregion

    private static int GetPositiveInt(IConfiguration configuration, string key, int defaultValue)
    {
        var value = configuration[key];

        return int.TryParse(value, out var parsedValue) && parsedValue > 0
            ? parsedValue
            : defaultValue;
    }

    private static NpgsqlDataSource BuildPostgresDataSource(IConfiguration configuration, string connectionName)
    {
        var connectionString = configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString)
        {
            ConnectionStringBuilder =
            {
                IncludeErrorDetail = true,
                Timeout = 100
            }
        };

        return dataSourceBuilder.Build();
    }

}
