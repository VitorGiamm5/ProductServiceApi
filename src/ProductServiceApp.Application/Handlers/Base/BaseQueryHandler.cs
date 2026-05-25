using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace ProductServiceApp.Application.Handlers.Base;

public abstract class BaseQueryHandler<TQuery, TResponse>(
        Channel<(TQuery, TaskCompletionSource<TResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseChannelHandler<TQuery, TResponse>(channel, scopeFactory)
    where TQuery : class
    where TResponse : class
{
    protected override async Task<TResponse> HandleAsync(
        TQuery query, IServiceProvider services, CancellationToken ct)
    {
        try
        {
            return await ExecuteQueryAsync(query, services, ct);
        }
        catch (KeyNotFoundException) { throw; }
        catch (ArgumentException) { throw; }
    }

    protected abstract Task<TResponse> ExecuteQueryAsync(
        TQuery query, IServiceProvider services, CancellationToken ct);
}
