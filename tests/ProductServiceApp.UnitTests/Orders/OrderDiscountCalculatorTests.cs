using FluentAssertions;
using ProductServiceApp.Application.Business.Orders.Discounts;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;

namespace ProductServiceApp.UnitTests.Orders;

public class OrderDiscountCalculatorTests
{
    private readonly OrderDiscountCalculator _calculator = new();

    [Fact]
    public void Calculate_Should_Apply_20_Percent_When_Order_Has_Sandwich_Fries_And_Refreshment()
    {
        var result = _calculator.Calculate(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Fries, 10m),
                Product(ProductsTypeEnum.Refreshment, 5m)
            ],
            Rules());

        result.SubTotalValue.Should().Be(35m);
        result.DiscountPercentage.Should().Be(20m);
        result.DiscountValue.Should().Be(7m);
        result.TotalValue.Should().Be(28m);
    }

    [Fact]
    public void Calculate_Should_Apply_15_Percent_When_Order_Has_Sandwich_And_Refreshment()
    {
        var result = _calculator.Calculate(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Refreshment, 10m)
            ],
            Rules());

        result.SubTotalValue.Should().Be(30m);
        result.DiscountPercentage.Should().Be(15m);
        result.DiscountValue.Should().Be(4.50m);
        result.TotalValue.Should().Be(25.50m);
    }

    [Fact]
    public void Calculate_Should_Apply_10_Percent_When_Order_Has_Sandwich_And_Fries()
    {
        var result = _calculator.Calculate(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Fries, 10m)
            ],
            Rules());

        result.SubTotalValue.Should().Be(30m);
        result.DiscountPercentage.Should().Be(10m);
        result.DiscountValue.Should().Be(3m);
        result.TotalValue.Should().Be(27m);
    }

    [Fact]
    public void Calculate_Should_Not_Apply_Discount_When_No_Rule_Matches()
    {
        var result = _calculator.Calculate(
            [Product(ProductsTypeEnum.Fries, 10m)],
            Rules());

        result.SubTotalValue.Should().Be(10m);
        result.DiscountPercentage.Should().Be(0m);
        result.DiscountValue.Should().Be(0m);
        result.TotalValue.Should().Be(10m);
    }

    [Fact]
    public void Calculate_Should_Reject_Duplicated_Product_Types()
    {
        var act = () => _calculator.Calculate(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Sandwich, 18m)
            ],
            Rules());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*duplicados do tipo Sanduiche*");
    }

    private static ProductEntity Product(ProductsTypeEnum type, decimal price)
    {
        return new ProductEntity
        {
            Id = (long)type,
            Name = type.ToString(),
            Type = type,
            Price = price
        };
    }

    private static List<OrderDiscountRuleEntity> Rules()
    {
        return
        [
            Rule(true, true, true, 20m),
            Rule(true, false, true, 15m),
            Rule(true, true, false, 10m)
        ];
    }

    private static OrderDiscountRuleEntity Rule(
        bool hasSandwich,
        bool hasFries,
        bool hasRefreshment,
        decimal discountPercentage)
    {
        return new OrderDiscountRuleEntity
        {
            HasSandwich = hasSandwich,
            HasFries = hasFries,
            HasRefreshment = hasRefreshment,
            DiscountPercentage = discountPercentage,
            IsActive = true,
            IsDeleted = false
        };
    }
}
