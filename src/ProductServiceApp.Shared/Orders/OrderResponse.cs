using ProductServiceApp.Shared.Products;

namespace ProductServiceApp.Shared.Orders;

public class OrderResponse : CreateOrderRequest
{
    public List<ProductResponse> Products { get; set; } = [];
    public DateTime? CreatedDate { get; set; }
    public decimal SubTotalValue { get; set; }
    public decimal TotalValue { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountValue { get; set; }
}
