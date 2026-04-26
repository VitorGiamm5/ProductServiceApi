using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Application.Products.Commands.Create;
using ProductServiceApp.Application.Products.Commands.Delete;
using ProductServiceApp.Application.Products.Commands.Update;
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
            Channel.CreateUnbounded<(GetAllProductQuery, TaskCompletionSource<IEnumerable<ProductResponse>>, CancellationToken)>()
        );

        services.AddSingleton(
            Channel.CreateUnbounded<(GetProductByIdQuery, TaskCompletionSource<ProductResponse>, CancellationToken)>()
        );

        // Handlers com replicas

        static void AddWorkers<T>(IServiceCollection services, int count) where T : BackgroundService
        {
            for (int i = 0; i < count; i++)
                services.AddSingleton<IHostedService>(sp => ActivatorUtilities.CreateInstance<T>(sp)); // esse  ActivatorUtilities.CreateInstance<T>(sp) que esta dando erro
        }

        // Uso:
        AddWorkers<CreateProductCommandHandler>(services, 2);
        AddWorkers<UpdateProductCommandHandler>(services, 2);
        AddWorkers<DeleteProductCommandHandler>(services, 2);
        AddWorkers<GetAllProductQueryHandler>(services, 1);
        AddWorkers<GetProductByIdQueryHandler>(services, 1);

        #endregion
    }
}
