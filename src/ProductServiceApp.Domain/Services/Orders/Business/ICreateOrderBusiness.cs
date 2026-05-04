using ProductServiceApp.Domain.Services.Base;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Domain.Services.Orders.Business;

public interface ICreateOrderBusiness : IBaseBusinessService<CreateOrderCommand, OrderResponse>
{
}
