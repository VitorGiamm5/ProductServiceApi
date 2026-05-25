using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Handlers;

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
