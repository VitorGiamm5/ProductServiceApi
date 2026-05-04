using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Orders;

namespace ProductServiceApp.Application.Business.Orders.GetAll;

public class GetAllOrderBusiness(IOrderQueryRepository repository)
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
        return await repository.GetAllAsync(ct);
    }

    #endregion

    #region OUTBOX

    protected override Task<IEnumerable<OrderResponse>> PostProcessAsync(IEnumerable<OrderEntity> result, CancellationToken ct)
    {
        return Task.FromResult(result.Select(order => new OrderResponse(order)));
    }

    #endregion

}
