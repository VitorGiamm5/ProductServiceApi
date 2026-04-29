using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Orders.Commands.Delete;

public class DeleteOrderCommandHandler(
        Channel<(DeleteOrderCommand, TaskCompletionSource<BooleanResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseCommandHandler<DeleteOrderCommand, BooleanResponse>(channel, scopeFactory)
{
    protected override async Task<BooleanResponse> ExecuteCommandAsync(
        DeleteOrderCommand command,
        IServiceProvider services,
        CancellationToken ct)
    {
        var business = services.GetRequiredService<IDeleteOrderBusiness>();

        return await business.ExecuteAsync(command, ct);
    }
}
