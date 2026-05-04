using ProductServiceApp.Domain.Entities.Products;

namespace ProductServiceApp.Domain.Entities.Orders;

public class OrderProductEntity
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public OrderEntity? Order { get; set; }
    public ProductEntity? Product { get; set; }
}
