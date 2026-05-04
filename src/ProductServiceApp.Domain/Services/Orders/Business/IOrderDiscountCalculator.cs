using ProductServiceApp.Domain.Services.Base;
using ProductServiceApp.Domain.Services.Orders.AdditionalFeaturesBusiness.OrderDiscount;

namespace ProductServiceApp.Domain.Services.Orders.Business;

public interface IOrderDiscountCalculator : IBaseBusinessService<OrderDiscountRequest, OrderDiscountResult>
{
}
