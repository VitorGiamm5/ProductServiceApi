using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Domain.Products.Dtos;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : BackgroundService
{
    private readonly Channel<(CreateProductCommand, TaskCompletionSource<ProductsResponse>, CancellationToken)> _channel;
    private readonly IServiceScopeFactory _scopeFactory;

    public CreateProductCommandHandler(
        Channel<(CreateProductCommand, TaskCompletionSource<ProductsResponse>, CancellationToken)> channel,
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

                ProductsResponse response = new()
                {
                    Id = 123,
                    Name = command.Name,
                    Price = command.Price,
                    Type = command.Type
                };

                await Task.Delay(10000, cts.Token);

                tcs.TrySetResult(response);
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
