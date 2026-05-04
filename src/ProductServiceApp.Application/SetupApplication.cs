using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Application.Business.Products.Create;
using ProductServiceApp.Application.Business.Products.Delete;
using ProductServiceApp.Application.Business.Products.GetAll;
using ProductServiceApp.Application.Business.Products.GetById;
using ProductServiceApp.Application.Business.Products.GetByIdList;
using ProductServiceApp.Application.Business.Products.Update;
using ProductServiceApp.Application.Business.Orders.Create;
using ProductServiceApp.Application.Business.Orders.Delete;
using ProductServiceApp.Application.Business.Orders.GetAll;
using ProductServiceApp.Application.Business.Orders.GetById;
using ProductServiceApp.Application.Business.Orders.Update;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Application.Cache.Products;
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
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using System.Threading.Channels;
using ProductServiceApp.Application.Business.Orders.OrderDiscount;

namespace ProductServiceApp.Application;

public static class SetupApplication
{
    private const int _defaultQueueMessageQuantity = 100;
    private const int _defaultWorkersReplicas = 1;

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        #region Cache

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetValue<string>("Redis:ConnectionString")
                ?? "localhost:6379";
            options.InstanceName = configuration.GetValue<string>("Redis:InstanceName")
                ?? "ProductServiceApp:";
        });

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

        // Setup application
        SetupProductApplication(services, configuration);
        SetupOrderApplication(services, configuration);

        return services;
    }

    #region Product application setup

    private static void SetupProductApplication(IServiceCollection services, IConfiguration configuration)
    {
        #region Channels — Commands (bounded with backpressure)

        var queueCapacity = configuration.GetSection("QueueCapacity");

        services.AddSingleton(Channel.CreateBounded<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueCreateProduct", _defaultQueueMessageQuantity))));

        services.AddSingleton(Channel.CreateBounded<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueUpdateProduct", _defaultQueueMessageQuantity))));

        services.AddSingleton(Channel.CreateBounded<(DeleteProductCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueDeleteProduct", _defaultQueueMessageQuantity))));

        #endregion

        #region Channels — Queries (unbounded)

        services.AddSingleton(Channel.CreateUnbounded<(GetAllProductQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)>());

        services.AddSingleton(Channel.CreateUnbounded<(GetByIdProductQuery, TaskCompletionSource<ProductResponse>, CancellationToken)>());

        #endregion

        #region Handlers — Workers with replicas

        var workersReplicasSection = configuration.GetSection("WorkersReplicas");

        services.AddWorkers<CreateProductCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasCreateProduct", _defaultWorkersReplicas));
        services.AddWorkers<UpdateProductCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasUpdateProduct", _defaultWorkersReplicas));
        services.AddWorkers<DeleteProductCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasDeleteProduct", _defaultWorkersReplicas));
        services.AddWorkers<GetAllProductQueryHandler>(workersReplicasSection.GetValue<int>("ReplicasGetAllProduct", _defaultWorkersReplicas));
        services.AddWorkers<GetByIdProductQueryHandler>(workersReplicasSection.GetValue<int>("ReplicasGetByIdProduct", _defaultWorkersReplicas));

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
            BoundedOptions(queueCapacity.GetValue<int>("QueueCreateOrder", _defaultQueueMessageQuantity))));

        services.AddSingleton(Channel.CreateBounded<(UpdateOrderCommand, TaskCompletionSource<OrderResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueUpdateOrder", _defaultQueueMessageQuantity))));

        services.AddSingleton(Channel.CreateBounded<(DeleteOrderCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueDeleteOrder", _defaultQueueMessageQuantity))));

        #endregion

        #region Channels — Queries (unbounded)

        services.AddSingleton(Channel.CreateUnbounded<(GetAllOrderQuery, TaskCompletionSource<IEnumerable<OrderResponse>>, CancellationToken)>());

        services.AddSingleton(Channel.CreateUnbounded<(GetByIdOrderQuery, TaskCompletionSource<OrderResponse>, CancellationToken)>());

        #endregion

        #region Handlers — Workers with replicas

        var workersReplicasSection = configuration.GetSection("WorkersReplicas");

        services.AddWorkers<CreateOrderCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasCreateOrder", _defaultWorkersReplicas));
        services.AddWorkers<UpdateOrderCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasUpdateOrder", _defaultWorkersReplicas));
        services.AddWorkers<DeleteOrderCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasDeleteOrder", _defaultWorkersReplicas));
        services.AddWorkers<GetAllOrderQueryHandler>(workersReplicasSection.GetValue<int>("ReplicasGetAllOrder", _defaultWorkersReplicas));
        services.AddWorkers<GetByIdOrderQueryHandler>(workersReplicasSection.GetValue<int>("ReplicasGetByIdOrder", _defaultWorkersReplicas));

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

    private static void AddWorkers<T>(this IServiceCollection services, int count)
        where T : class, IHostedService
    {
        for (int i = 0; i < count; i++)
            services.AddSingleton<IHostedService, T>();
    }

    #endregion
}
