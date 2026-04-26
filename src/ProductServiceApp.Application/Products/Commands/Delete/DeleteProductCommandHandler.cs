using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Products.Commands.Delete;

public class DeleteProductCommandHandler : BackgroundService
{
    private readonly Channel<(DeleteProductCommand, TaskCompletionSource<bool>, CancellationToken)> _channel;
    private readonly IServiceScopeFactory _scopeFactory;

    public DeleteProductCommandHandler(
        Channel<(DeleteProductCommand, TaskCompletionSource<bool>, CancellationToken)> channel, 
        IServiceScopeFactory scopeFactory
        )
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

                await Task.Delay(10000, cts.Token);

                tcs.TrySetResult(true);
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
