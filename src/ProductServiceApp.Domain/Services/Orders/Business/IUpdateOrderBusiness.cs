using ProductServiceApp.Domain.Services.Base;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Domain.Services.Orders.Business;

public interface IUpdateOrderBusiness : IBaseBusinessService<UpdateOrderCommand, OrderResponse>
{
}
