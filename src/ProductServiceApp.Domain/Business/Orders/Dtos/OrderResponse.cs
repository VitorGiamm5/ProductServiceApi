using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Entities.Orders;

namespace ProductServiceApp.Domain.Business.Orders.Dtos;

public class OrderResponse : CreateOrderRequest
{
    public OrderResponse(OrderEntity input) => MapFrom(input);

    public List<ProductResponse> Products { get; set; } = [];
    public DateTime? CreatedDate { get; set; }
    public decimal SubTotalValue { get; set; }
    public decimal TotalValue { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountValue { get; set; }

    public OrderResponse MapFrom(OrderEntity? input)
    {
        if (input is null)
        {
            return this;
        }

        Id = input.Id;
        ProductIds = [.. input.OrderProducts.Select(item => item.ProductId)];
        Products = [.. input.OrderProducts
            .Where(item => item.Product is not null)
            .Select(item =>
            {
                item.Product!.Price = item.UnitPrice;
                return new ProductResponse(item.Product);
            })];
        CreatedDate = input.OrdersAudit?.CreatedDate ?? input.CreatedDate;
        SubTotalValue = input.SubTotalValue;
        TotalValue = input.TotalValue;
        DiscountPercentage = input.DiscountPercentage;
        DiscountValue = input.DiscountValue;
        IsActive = input.IsActive;
        IsDeleted = input.IsDeleted;

        return this;
    }
}
