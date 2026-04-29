using ProductServiceApp.Domain.Entities.Base;
using ProductServiceApp.Domain.Identifications;

namespace ProductServiceApp.Domain.Entities.Orders;

public class OrderDiscountRuleEntity : BaseAuditEntity, IIdentifiableLong
{
    public long Id { get; set; }
    public bool HasSandwich { get; set; }
    public bool HasFries { get; set; }
    public bool HasRefreshment { get; set; }
    public decimal DiscountPercentage { get; set; }
}
