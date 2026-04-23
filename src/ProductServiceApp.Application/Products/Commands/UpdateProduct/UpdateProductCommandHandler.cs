using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Domain.Products.Dtos;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : BackgroundService
{
    private readonly Channel<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> _channel;
    private readonly IServiceScopeFactory _scopeFactory;

    public UpdateProductCommandHandler(Channel<(UpdateProductCommand, TaskCompletionSource<ProductResponse>, CancellationToken)> channel,
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

                ProductResponse response = new()
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
