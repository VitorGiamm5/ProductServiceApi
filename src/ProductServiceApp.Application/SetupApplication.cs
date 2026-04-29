using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Application.Business.Products.Create;
using ProductServiceApp.Application.Business.Products.Delete;
using ProductServiceApp.Application.Business.Products.GetAll;
using ProductServiceApp.Application.Business.Products.GetById;
using ProductServiceApp.Application.Business.Products.Update;
using ProductServiceApp.Application.Business.Orders.Create;
using ProductServiceApp.Application.Business.Orders.Delete;
using ProductServiceApp.Application.Business.Orders.Discounts;
using ProductServiceApp.Application.Business.Orders.GetAll;
using ProductServiceApp.Application.Business.Orders.GetById;
using ProductServiceApp.Application.Business.Orders.Update;
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

namespace ProductServiceApp.Application;

public static class SetupApplication
{
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
            BoundedOptions(queueCapacity.GetValue<int>("QueueCreateProduct", 100))));

        services.AddSingleton(Channel.CreateBounded<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueUpdateProduct", 100))));

        services.AddSingleton(Channel.CreateBounded<(DeleteProductCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueDeleteProduct", 100))));

        #endregion

        #region Channels — Queries (unbounded)

        services.AddSingleton(Channel.CreateUnbounded<(GetAllProductQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)>());

        services.AddSingleton(Channel.CreateUnbounded<(GetByIdProductQuery, TaskCompletionSource<ProductResponse>, CancellationToken)>());

        #endregion

        #region Handlers — Workers with replicas

        var workersReplicasSection = configuration.GetSection("WorkersReplicas");

        services.AddWorkers<CreateProductCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasCreateProduct", 2));
        services.AddWorkers<UpdateProductCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasUpdateProduct", 2));
        services.AddWorkers<DeleteProductCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasDeleteProduct", 2));
        services.AddWorkers<GetAllProductQueryHandler>(workersReplicasSection.GetValue<int>("ReplicasGetAllProduct", 1));
        services.AddWorkers<GetByIdProductQueryHandler>(workersReplicasSection.GetValue<int>("ReplicasGetByIdProduct", 1));

        #endregion

        #region Services — Business

        services.AddScoped<IGetAllProductBusiness, GetAllProductBusiness>();
        services.AddScoped<IGetByIdProductBusiness, GetByIdProductBusiness>();
        services.AddScoped<ICreateProductBusiness, CreateProductBusiness>();
        services.AddScoped<IUpdateProductBusiness, UpdateProductBusiness>();
        services.AddScoped<IDeleteProductBusiness, DeleteProductBusiness>();

        #endregion
    }

    #endregion

    #region Order application setup

    private static void SetupOrderApplication(IServiceCollection services, IConfiguration configuration)
    {
        var queueCapacity = configuration.GetSection("QueueCapacity");

        services.AddSingleton(Channel.CreateBounded<(CreateOrderCommand, TaskCompletionSource<OrderResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueCreateOrder", 100))));

        services.AddSingleton(Channel.CreateBounded<(UpdateOrderCommand, TaskCompletionSource<OrderResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueUpdateOrder", 100))));

        services.AddSingleton(Channel.CreateBounded<(DeleteOrderCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)>(
            BoundedOptions(queueCapacity.GetValue<int>("QueueDeleteOrder", 100))));

        services.AddSingleton(Channel.CreateUnbounded<(GetAllOrderQuery, TaskCompletionSource<IEnumerable<OrderResponse>>, CancellationToken)>());

        services.AddSingleton(Channel.CreateUnbounded<(GetByIdOrderQuery, TaskCompletionSource<OrderResponse>, CancellationToken)>());

        var workersReplicasSection = configuration.GetSection("WorkersReplicas");

        services.AddWorkers<CreateOrderCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasCreateOrder", 2));
        services.AddWorkers<UpdateOrderCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasUpdateOrder", 2));
        services.AddWorkers<DeleteOrderCommandHandler>(workersReplicasSection.GetValue<int>("ReplicasDeleteOrder", 2));
        services.AddWorkers<GetAllOrderQueryHandler>(workersReplicasSection.GetValue<int>("ReplicasGetAllOrder", 1));
        services.AddWorkers<GetByIdOrderQueryHandler>(workersReplicasSection.GetValue<int>("ReplicasGetByIdOrder", 1));

        services.AddScoped<IOrderDiscountCalculator, OrderDiscountCalculator>();
        services.AddScoped<IGetAllOrderBusiness, GetAllOrderBusiness>();
        services.AddScoped<IGetByIdOrderBusiness, GetByIdOrderBusiness>();
        services.AddScoped<ICreateOrderBusiness, CreateOrderBusiness>();
        services.AddScoped<IUpdateOrderBusiness, UpdateOrderBusiness>();
        services.AddScoped<IDeleteOrderBusiness, DeleteOrderBusiness>();
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
