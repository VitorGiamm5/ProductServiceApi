using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;

namespace ProductServiceApp.Application.Business.Orders.Discounts;

public sealed class OrderDraft
{
    public long Id { get; set; }
    public List<ProductEntity> Products { get; set; } = [];
    public List<OrderDiscountRuleEntity> DiscountRules { get; set; } = [];
    public bool? IsActive { get; set; } = true;
    public bool? IsDeleted { get; set; } = false;
}
