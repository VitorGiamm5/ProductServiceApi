using ProductServiceApp.Domain.Entities.Base;
using ProductServiceApp.Domain.Identifications;

namespace ProductServiceApp.Domain.Entities.Orders;

public class OrderAuditEntity : BaseAuditEntity, IIdentifiableLong
{
    public long Id { get; set; }
}
