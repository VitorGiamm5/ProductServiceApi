using ProductServiceApp.Domain.Services.Base;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Domain.Services.Orders.Business;

public interface IDeleteOrderBusiness : IBaseBusinessService<DeleteOrderCommand, BooleanResponse>
{
}
