using ProductServiceApp.Domain.Business.Base;
using ProductServiceApp.Domain.Business.Orders.AdditionalFeaturesBusiness.OrderDiscount;

namespace ProductServiceApp.Domain.Business.Orders.Business;

public interface IOrderDiscountCalculator : IBaseBusinessService<OrderDiscountRequest, OrderDiscountResult>
{
}
