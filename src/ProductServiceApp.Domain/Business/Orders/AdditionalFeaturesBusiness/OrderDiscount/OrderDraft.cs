using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Identifications;

namespace ProductServiceApp.Domain.Business.Orders.AdditionalFeaturesBusiness.OrderDiscount;

public sealed class OrderDraft : IIdentifiableLong
{
    public long Id { get; set; }
    public IReadOnlyCollection<ProductEntity> Products { get; set; } = [];
    public IReadOnlyCollection<OrderDiscountRuleEntity> DiscountRules { get; set; } = [];
    public OrderDiscountResult Result { get; set; }
    public OrderEntity Entity { get; set; }
    public bool? IsActive { get; set; } = true;
    public bool? IsDeleted { get; set; } = false;
    public DateTime? CreatedDate { get; set; }
}
