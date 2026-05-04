using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Mappers;
using ProductServiceApp.Domain.Services.Products.Dtos;

namespace ProductServiceApp.Domain.Services.Orders.Dtos;

public class OrderResponse : CreateOrderRequest, IFromMapper<OrderResponse, OrderEntity>
{
    #region Constructors

    public OrderResponse(OrderEntity input) => MapFrom(input);

    #endregion

    #region Additional Properties

    public new List<ProductResponse> Products { get; set; } = [];
    public DateTime? CreatedDate { get; set; }
    public decimal SubTotalValue { get; set; }
    public decimal TotalValue { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountValue { get; set; }

    #endregion

    #region Mapping

    public OrderResponse MapFrom(OrderEntity? input)
    {
        if (input is null)
        {
            return this;
        }

        Id = input.Id;
        Products = [.. input.OrderProducts
            .Where(item => item.Product is not null)
            .Select(item =>
            {
                item.Product!.Price = item.UnitPrice;
                return new ProductResponse(item.Product)
                {
                    Quantity = item.Quantity
                };
            })];
        CreatedDate = input.OrdersAudit?.CreatedDate;
        SubTotalValue = input.SubTotalValue;
        TotalValue = input.TotalValue;
        DiscountPercentage = input.DiscountPercentage;
        DiscountValue = input.DiscountValue;
        IsActive = input.IsActive;
        IsDeleted = input.IsDeleted;

        return this;
    }

    #endregion

}
