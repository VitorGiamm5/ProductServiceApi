using ProductServiceApp.Domain.Business.Base;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;

namespace ProductServiceApp.Domain.Business.Orders.Business;

public interface IDeleteOrderBusiness : IBaseBusinessService<DeleteOrderCommand, BooleanResponse>
{
}
