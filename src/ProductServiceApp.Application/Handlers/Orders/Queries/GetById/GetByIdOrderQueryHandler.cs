using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Orders.Queries.GetById;

public class GetByIdOrderQueryHandler(
        Channel<(GetByIdOrderQuery, TaskCompletionSource<OrderResponse>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseQueryHandler<GetByIdOrderQuery, OrderResponse>(channel, scopeFactory)
{
    protected override async Task<OrderResponse> ExecuteQueryAsync(
        GetByIdOrderQuery query,
        IServiceProvider services,
        CancellationToken ct)
    {
        var business = services.GetRequiredService<IGetByIdOrderBusiness>();

        return await business.ExecuteAsync(query, ct);
    }
}
