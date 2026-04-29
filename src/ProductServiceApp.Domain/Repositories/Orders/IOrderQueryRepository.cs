using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Repositories.Base;

namespace ProductServiceApp.Domain.Repositories.Orders;

public interface IOrderQueryRepository : IBaseQueryRepository<OrderEntity>
{
}
