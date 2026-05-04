using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;

namespace ProductServiceApp.Domain.Services.Orders.AdditionalFeaturesBusiness.OrderDiscount;

public sealed record OrderDiscountProduct(
    ProductEntity Product,
    int Quantity);

public sealed class OrderDiscountRequest
{
    public IReadOnlyCollection<OrderDiscountProduct> Products { get; set; } = [];
    public IReadOnlyCollection<OrderDiscountRuleEntity> Rules { get; set; } = [];
}
