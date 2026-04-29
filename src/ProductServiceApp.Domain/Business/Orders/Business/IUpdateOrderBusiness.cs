using ProductServiceApp.Domain.Business.Base;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;

namespace ProductServiceApp.Domain.Business.Orders.Business;

public interface IUpdateOrderBusiness : IBaseBusinessService<UpdateOrderCommand, OrderResponse>
{
}
