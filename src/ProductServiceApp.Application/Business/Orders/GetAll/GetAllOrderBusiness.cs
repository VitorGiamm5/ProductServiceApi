using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Application.Business.Orders.GetAll;

public class GetAllOrderBusiness(
        IOrderQueryRepository repository,
        IOrderCacheService cache)
    : BaseBusinessService<GetAllOrderQuery, GetAllOrderQuery, IEnumerable<OrderEntity>, IEnumerable<OrderResponse>>,
    IGetAllOrderBusiness
{
    #region INBOX

    protected override Task<GetAllOrderQuery> PreProcessAsync(GetAllOrderQuery input, CancellationToken ct)
    {
        return Task.FromResult(input);
    }

    #endregion

    #region PROCESS

    protected override async Task<IEnumerable<OrderEntity>> ProcessAsync(GetAllOrderQuery input, CancellationToken ct)
    {
        var cachedOrders = await cache.GetAllAsync(ct);
        if (cachedOrders is not null)
            return cachedOrders;

        var orders = (await repository.GetAllAsync(ct)).ToArray();
        await cache.SetAllAsync(orders, ct);

        return orders;
    }

    #endregion

    #region OUTBOX

    protected override Task<IEnumerable<OrderResponse>> PostProcessAsync(IEnumerable<OrderEntity> result, CancellationToken ct)
    {
        return Task.FromResult(result.Select(order => new OrderResponse(order)));
    }

    #endregion

}
