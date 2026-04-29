using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Orders.Commands.Create;

public class CreateOrderCommandHandler(
        Channel<(CreateOrderCommand, TaskCompletionSource<OrderResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseCommandHandler<CreateOrderCommand, OrderResponse>(channel, scopeFactory)
{
    protected override async Task<OrderResponse> ExecuteCommandAsync(
        CreateOrderCommand command,
        IServiceProvider services,
        CancellationToken ct)
    {
        var business = services.GetRequiredService<ICreateOrderBusiness>();

        return await business.ExecuteAsync(command, ct);
    }
}
