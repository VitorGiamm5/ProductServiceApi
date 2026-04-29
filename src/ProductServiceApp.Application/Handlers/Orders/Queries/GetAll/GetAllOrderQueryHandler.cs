using Microsoft.Extensions.DependencyInjection;
using ProductServiceApp.Application.Handlers.Base;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using System.Threading.Channels;

namespace ProductServiceApp.Application.Handlers.Orders.Queries.GetAll;

public class GetAllOrderQueryHandler(
        Channel<(GetAllOrderQuery, TaskCompletionSource<IEnumerable<OrderResponse>>, CancellationToken)> channel,
        IServiceScopeFactory scopeFactory)
    : BaseQueryHandler<GetAllOrderQuery, IEnumerable<OrderResponse>>(channel, scopeFactory)
{
    protected override async Task<IEnumerable<OrderResponse>> ExecuteQueryAsync(
        GetAllOrderQuery query,
        IServiceProvider services,
        CancellationToken ct)
    {
        var business = services.GetRequiredService<IGetAllOrderBusiness>();

        return await business.ExecuteAsync(query, ct);
    }
}
