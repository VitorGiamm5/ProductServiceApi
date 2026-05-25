using System.Threading.Channels;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Application.Business.Orders.Create;
using ProductServiceApp.Application.Business.Orders.Delete;
using ProductServiceApp.Application.Business.Orders.GetAll;
using ProductServiceApp.Application.Business.Orders.GetById;
using ProductServiceApp.Application.Business.Orders.OrderDiscount;
using ProductServiceApp.Application.Business.Orders.Update;
using ProductServiceApp.Application.Business.Products.Create;
using ProductServiceApp.Application.Business.Products.Delete;
using ProductServiceApp.Application.Business.Products.GetAll;
using ProductServiceApp.Application.Business.Products.GetById;
using ProductServiceApp.Application.Business.Products.GetByIdList;
using ProductServiceApp.Application.Business.Products.Update;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Application.Cache.Redis;
using ProductServiceApp.Application.Cache.Warmup;
using ProductServiceApp.Application.Handlers.Orders.Commands.Create;
using ProductServiceApp.Application.Handlers.Orders.Commands.Delete;
using ProductServiceApp.Application.Handlers.Orders.Commands.Update;
using ProductServiceApp.Application.Handlers.Orders.Queries.GetAll;
using ProductServiceApp.Application.Handlers.Orders.Queries.GetById;
using ProductServiceApp.Application.Handlers.Products.Commands.Create;
using ProductServiceApp.Application.Handlers.Products.Commands.Delete;
using ProductServiceApp.Application.Handlers.Products.Commands.Update;
using ProductServiceApp.Application.Handlers.Products.Queries.GetAll;
using ProductServiceApp.Application.Handlers.Products.Queries.GetById;
using ProductServiceApp.Application.Metrics;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;
using ProductServiceApp.Domain.Services.Products.Business;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application;

public static class SetupApplication
{
    private const int _defaultQueueMessageQuantity = 100;
    private const int _defaultWorkersReplicas = 1;

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        #region Cache

        services.Configure<RedisCacheOptions>(configuration.GetSection("Redis"));
        services.Configure<CacheWarmupOptions>(configuration.GetSection("CacheWarmup"));

        services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
        services.AddHostedService<RedisCacheWarmupService>();
        services.AddScoped<ICacheWarmupFeature, ProductsCacheWarmupFeature>();
        services.AddScoped<ICacheWarmupFeature, OrdersCacheWarmupFeature>();
        services.AddHostedService<CacheWarmupHostedService>();
        services.AddScoped<IProductCacheService, ProductCacheService>();
        services.AddScoped<IOrderCacheService, OrderCacheService>();

        #endregion

        #region Validators — Auto reflection

        services.AddValidatorsFromAssembly(typeof(SetupApplication).Assembly);

        #endregion

        #region Metrics

        services.AddSingleton<ChannelMetricsService>();
        services.AddHostedService(sp => sp.GetRequiredService<ChannelMetricsService>());

        #endregion

        #region Setup applications (Business)

        SetupProductApplication(services, configuration);
        SetupOrderApplication(services, configuration);

        #endregion

        return services;
    }

    #region Product application setup

    private static void SetupProductApplication(IServiceCollection services, IConfiguration configuration)
    {
        #region Channels — Commands (bounded with backpressure)

        var queueCapacity = configuration.GetSection("QueueCapacity");

        services.AddSingleton(Channel.CreateBounded<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)>(
            BoundedOptions(GetPositiveInt(queueCapacity, "QueueCreateProduct", _defaultQueueMessageQuantity))));

        services.AddSingleton(Channel.CreateBounded<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)>(
            BoundedOptions(GetPositiveInt(queueCapacity, "QueueUpdateProduct", _defaultQueueMessageQuantity))));

        services.AddSingleton(Channel.CreateBounded<(DeleteProductCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)>(
            BoundedOptions(GetPositiveInt(queueCapacity, "QueueDeleteProduct", _defaultQueueMessageQuantity))));

        #endregion

        #region Channels — Queries (unbounded)

        services.AddSingleton(Channel.CreateUnbounded<(GetAllProductQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)>());

        services.AddSingleton(Channel.CreateUnbounded<(GetByIdProductQuery, TaskCompletionSource<ProductResponse>, CancellationToken)>());

        #endregion

        #region Handlers — Workers with replicas

        var workersReplicasSection = configuration.GetSection("WorkersReplicas");

        services.AddWorkers<CreateProductCommandHandler>(GetPositiveInt(workersReplicasSection, "ReplicasCreateProduct", _defaultWorkersReplicas));
        services.AddWorkers<UpdateProductCommandHandler>(GetPositiveInt(workersReplicasSection, "ReplicasUpdateProduct", _defaultWorkersReplicas));
        services.AddWorkers<DeleteProductCommandHandler>(GetPositiveInt(workersReplicasSection, "ReplicasDeleteProduct", _defaultWorkersReplicas));
        services.AddWorkers<GetAllProductQueryHandler>(GetPositiveInt(workersReplicasSection, "ReplicasGetAllProduct", _defaultWorkersReplicas));
        services.AddWorkers<GetByIdProductQueryHandler>(GetPositiveInt(workersReplicasSection, "ReplicasGetByIdProduct", _defaultWorkersReplicas));

        #endregion

        #region Services — Business

        services.AddScoped<IGetAllProductBusiness, GetAllProductBusiness>();
        services.AddScoped<IGetByIdProductBusiness, GetByIdProductBusiness>();
        services.AddScoped<LoadProductsAsync>();
        services.AddScoped<ICreateProductBusiness, CreateProductBusiness>();
        services.AddScoped<IUpdateProductBusiness, UpdateProductBusiness>();
        services.AddScoped<IDeleteProductBusiness, DeleteProductBusiness>();

        #endregion
    }

    #endregion

    #region Order application setup

    private static void SetupOrderApplication(IServiceCollection services, IConfiguration configuration)
    {
        #region Channels — Commands (bounded with backpressure)

        var queueCapacity = configuration.GetSection("QueueCapacity");

        services.AddSingleton(Channel.CreateBounded<(CreateOrderCommand, TaskCompletionSource<OrderResponse>, CancellationToken)>(
            BoundedOptions(GetPositiveInt(queueCapacity, "QueueCreateOrder", _defaultQueueMessageQuantity))));

        services.AddSingleton(Channel.CreateBounded<(UpdateOrderCommand, TaskCompletionSource<OrderResponse>, CancellationToken)>(
            BoundedOptions(GetPositiveInt(queueCapacity, "QueueUpdateOrder", _defaultQueueMessageQuantity))));

        services.AddSingleton(Channel.CreateBounded<(DeleteOrderCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)>(
            BoundedOptions(GetPositiveInt(queueCapacity, "QueueDeleteOrder", _defaultQueueMessageQuantity))));

        #endregion

        #region Channels — Queries (unbounded)

        services.AddSingleton(Channel.CreateUnbounded<(GetAllOrderQuery, TaskCompletionSource<IEnumerable<OrderResponse>>, CancellationToken)>());

        services.AddSingleton(Channel.CreateUnbounded<(GetByIdOrderQuery, TaskCompletionSource<OrderResponse>, CancellationToken)>());

        #endregion

        #region Handlers — Workers with replicas

        var workersReplicasSection = configuration.GetSection("WorkersReplicas");

        services.AddWorkers<CreateOrderCommandHandler>(GetPositiveInt(workersReplicasSection, "ReplicasCreateOrder", _defaultWorkersReplicas));
        services.AddWorkers<UpdateOrderCommandHandler>(GetPositiveInt(workersReplicasSection, "ReplicasUpdateOrder", _defaultWorkersReplicas));
        services.AddWorkers<DeleteOrderCommandHandler>(GetPositiveInt(workersReplicasSection, "ReplicasDeleteOrder", _defaultWorkersReplicas));
        services.AddWorkers<GetAllOrderQueryHandler>(GetPositiveInt(workersReplicasSection, "ReplicasGetAllOrder", _defaultWorkersReplicas));
        services.AddWorkers<GetByIdOrderQueryHandler>(GetPositiveInt(workersReplicasSection, "ReplicasGetByIdOrder", _defaultWorkersReplicas));

        #endregion

        #region Services — Business

        services.AddScoped<IOrderDiscountCalculator, OrderDiscountCalculator>();
        services.AddScoped<IGetAllOrderBusiness, GetAllOrderBusiness>();
        services.AddScoped<IGetByIdOrderBusiness, GetByIdOrderBusiness>();
        services.AddScoped<ICreateOrderBusiness, CreateOrderBusiness>();
        services.AddScoped<IUpdateOrderBusiness, UpdateOrderBusiness>();
        services.AddScoped<IDeleteOrderBusiness, DeleteOrderBusiness>();

        #endregion
    }

    #endregion

    #region Private methods

    private static BoundedChannelOptions BoundedOptions(int capacity) => new(capacity)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = false,
        SingleWriter = false
    };

    private static int GetPositiveInt(IConfiguration configuration, string key, int defaultValue)
    {
        var value = configuration[key];

        return int.TryParse(value, out var parsedValue) && parsedValue > 0
            ? parsedValue
            : defaultValue;
    }

    private static void AddWorkers<T>(this IServiceCollection services, int count)
        where T : class, IHostedService
    {
        for (int i = 0; i < count; i++)
            services.AddSingleton<IHostedService, T>();
    }

    #endregion
}
