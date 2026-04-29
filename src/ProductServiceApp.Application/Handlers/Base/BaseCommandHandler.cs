using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Base;

public abstract class BaseCommandHandler<TCommand, TResponse>(
        Channel<(TCommand, TaskCompletionSource<TResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseChannelHandler<TCommand, TResponse>(channel, scopeFactory)
    where TCommand : class
    where TResponse : class
{
    protected override async Task<TResponse> HandleAsync(
        TCommand command, IServiceProvider services, CancellationToken ct)
    {
        try
        {
            return await ExecuteCommandAsync(command, services, ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Conflito de concorrência ao persistir os dados.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Erro ao persistir os dados.", ex);
        }
        catch (InvalidOperationException) { throw; }
        catch (ArgumentException) { throw; }
    }

    protected abstract Task<TResponse> ExecuteCommandAsync(
        TCommand command, IServiceProvider services, CancellationToken ct);
}