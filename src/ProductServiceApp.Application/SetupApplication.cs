using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Application.Products.Commands.CreateProduct;
using ProductServiceApp.Application.Products.Commands.DeleteProduct;
using ProductServiceApp.Application.Products.Commands.UpdateProduct;
using ProductServiceApp.Application.Products.Queries.GetAll;
using ProductServiceApp.Application.Products.Queries.GetById;
using ProductServiceApp.Domain.Products.Dtos;
using ProductServiceApp.Infrastructure;
using System.Threading.Channels;

namespace ProductServiceApp.Application;

public static class SetupApplication
{
    private static readonly int _queue_capacity = 1000;

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        SetupProductApplication(services);

        services.AddInfrastructure(configuration);

        return services;
    }

    private static void SetupProductApplication(IServiceCollection services)
    {
        #region Commands

        // Commands - Bounded com backpressure
        services.AddSingleton(
            Channel.CreateBounded<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)>(
                new BoundedChannelOptions(_queue_capacity)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = false,
                    SingleWriter = false
                }
            )
        );

        services.AddSingleton(
            Channel.CreateBounded<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)>(
                new BoundedChannelOptions(_queue_capacity)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = false,
                    SingleWriter = false
                }
            )
        );

        services.AddSingleton(
            Channel.CreateBounded<(DeleteProductCommand, TaskCompletionSource<bool>, CancellationToken)>(
                new BoundedChannelOptions(_queue_capacity)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = false,
                    SingleWriter = false
                }
            )
        );

        services.AddSingleton(
            Channel.CreateUnbounded<(GetAllProductsQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)>()
        );

        services.AddSingleton(
            Channel.CreateUnbounded<(GetProductByIdQuery, TaskCompletionSource<ProductResponse>, CancellationToken)>()
        );

        // Handlers com replicas

        static void AddWorkers<T>(IServiceCollection services, int count) where T : BackgroundService
        {
            for (int i = 0; i < count; i++)
                services.AddSingleton<IHostedService>(sp => ActivatorUtilities.CreateInstance<T>(sp));
        }

        // Uso:
        AddWorkers<CreateProductCommandHandler>(services, 2);
        AddWorkers<UpdateProductCommandHandler>(services, 2);
        AddWorkers<DeleteProductCommandHandler>(services, 2);
        AddWorkers<GetAllProductsQueryHandler>(services, 1);
        AddWorkers<GetProductByIdQueryHandler>(services, 1);

        #endregion
    }
}
