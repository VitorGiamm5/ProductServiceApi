using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;

namespace ProductServiceApp.Domain.Services.Orders.AdditionalFeaturesBusiness.OrderDiscount;

public sealed class OrderDiscountRequest
{
   public IReadOnlyCollection<ProductEntity> Products { get; set; }
   public IReadOnlyCollection<OrderDiscountRuleEntity> Rules { get; set; }
}
