using ProductServiceApp.Domain.Entities.Base;
using ProductServiceApp.Domain.Identifications;

namespace ProductServiceApp.Domain.Entities.Orders;

public class OrderEntity : BaseAuditEntity, IIdentifiableLong
{
    public long Id { get; set; }
    public long? IdOrdersAudit { get; set; }
    public decimal SubTotalValue { get; set; }
    public decimal TotalValue { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountValue { get; set; }
    public OrderAuditEntity? OrdersAudit { get; set; }
    public ICollection<OrderProductEntity> OrderProducts { get; set; } = [];
}
