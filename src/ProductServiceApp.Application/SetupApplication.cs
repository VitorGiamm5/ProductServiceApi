using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Application.Handlers.Products.Commands.Create;
using ProductServiceApp.Application.Handlers.Products.Commands.Delete;
using ProductServiceApp.Application.Handlers.Products.Commands.Update;
using ProductServiceApp.Application.Handlers.Products.Queries.GetAll;
using ProductServiceApp.Application.Handlers.Products.Queries.GetById;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Infrastructure;
using System.Threading.Channels;

namespace ProductServiceApp.Application;

public static class SetupApplication
{
    private static readonly int _queueCapacity = 1000;

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        SetupProductApplication(services);
        services.AddInfrastructure(configuration);
        return services;
    }

    private static void SetupProductApplication(IServiceCollection services)
    {
        #region Channels — Commands (bounded com backpressure)

        services.AddSingleton(Channel.CreateBounded<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)>(
            BoundedOptions(_queueCapacity)));

        services.AddSingleton(Channel.CreateBounded<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)>(
            BoundedOptions(_queueCapacity)));

        services.AddSingleton(Channel.CreateBounded<(DeleteProductCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)>(
            BoundedOptions(_queueCapacity)));

        #endregion

        #region Channels — Queries (unbounded)

        services.AddSingleton(Channel.CreateUnbounded<(GetAllProductQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)>());

        services.AddSingleton(Channel.CreateUnbounded<(GetByIdProductQuery, TaskCompletionSource<ProductResponse>, CancellationToken)>());

        #endregion

        #region Handlers — Workers com réplicas

        services.AddWorkers<CreateProductCommandHandler>(2);
        services.AddWorkers<UpdateProductCommandHandler>(2);
        services.AddWorkers<DeleteProductCommandHandler>(2);
        services.AddWorkers<GetAllProductQueryHandler>(1);
        services.AddWorkers<GetByIdProductQueryHandler>(1);

        #endregion
    }

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
}