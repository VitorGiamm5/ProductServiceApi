using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Domain.Products.Dtos;
using ProductServiceApp.Domain.Products.Entities;
using ProductServiceApp.Domain.Repositories.Products;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Products.Commands.Create;

public class CreateProductCommandHandler : BackgroundService
{
    private readonly Channel<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> _channel;
    private readonly IServiceScopeFactory _scopeFactory;

    public CreateProductCommandHandler(
        Channel<(CreateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (command, tcs, requestToken) in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            if (requestToken.IsCancellationRequested)
            {
                tcs.TrySetCanceled(requestToken);
                continue;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, requestToken);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IProductCommandRepository<ProductEntity>>();

                var entity = new CreateProductCommand(command);
                var response = await repository.CreateAsync(entity.MapTo(), cts.Token);

                tcs.TrySetResult(new ProductResponse());
            }
            catch (OperationCanceledException ex)
            {
                tcs.TrySetCanceled(ex.CancellationToken);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }
    }
}
