using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Application.Handlers.Orders.Commands.Update;

public class UpdateOrderCommandHandler(
        Channel<(UpdateOrderCommand, TaskCompletionSource<OrderResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseCommandHandler<UpdateOrderCommand, OrderResponse>(channel, scopeFactory)
{
    protected override async Task<OrderResponse> ExecuteCommandAsync(
        UpdateOrderCommand command,
        IServiceProvider services,
        CancellationToken ct)
    {
        var business = services.GetRequiredService<IUpdateOrderBusiness>();

        return await business.ExecuteAsync(command, ct);
    }
}
