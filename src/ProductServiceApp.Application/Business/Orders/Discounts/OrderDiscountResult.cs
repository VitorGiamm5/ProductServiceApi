namespace ProductServiceApp.Application.Business.Orders.Discounts;

public sealed class OrderDiscountResult
{
    public decimal SubTotalValue { get; init; }
    public decimal TotalValue { get; init; }
    public decimal DiscountPercentage { get; init; }
    public decimal DiscountValue { get; init; }
}
