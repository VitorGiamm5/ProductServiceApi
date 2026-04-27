using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Base;

public abstract class BaseChannelHandler<TQuery, TResponse> : BackgroundService
{
    private readonly Channel<(TQuery, TaskCompletionSource<TResponse>, CancellationToken)> _channel;
    protected readonly IServiceScopeFactory ScopeFactory;

    protected BaseChannelHandler(
        Channel<(TQuery, TaskCompletionSource<TResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        ScopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (query, tcs, requestToken) in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            if (requestToken.IsCancellationRequested)
            {
                tcs.TrySetCanceled(requestToken);
                continue;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, requestToken);
                await using var scope = ScopeFactory.CreateAsyncScope();

                var result = await HandleAsync(query, scope.ServiceProvider, cts.Token);
                tcs.TrySetResult(result);
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

    protected abstract Task<TResponse> HandleAsync(
        TQuery query, IServiceProvider services, CancellationToken ct);
}
