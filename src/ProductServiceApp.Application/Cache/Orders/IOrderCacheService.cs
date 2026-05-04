using ProductServiceApp.Domain.Entities.Orders;

namespace ProductServiceApp.Application.Cache.Orders;

public interface IOrderCacheService
{
    Task<OrderEntity[]?> GetAllAsync(CancellationToken cancellationToken);
    Task SetAllAsync(IEnumerable<OrderEntity> orders, CancellationToken cancellationToken);
    Task<OrderEntity?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task SetByIdAsync(OrderEntity order, CancellationToken cancellationToken);
    Task InvalidateAllAsync(CancellationToken cancellationToken);
    Task InvalidateByIdAsync(long id, CancellationToken cancellationToken);
}
