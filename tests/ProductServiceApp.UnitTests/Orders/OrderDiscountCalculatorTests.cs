using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ProductServiceApp.Application.Business.Orders.OrderDiscount;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Services.Orders.AdditionalFeaturesBusiness.OrderDiscount;

namespace ProductServiceApp.UnitTests.Orders;

public class OrderDiscountCalculatorTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Throw_ValidationException_When_Request_Is_Invalid()
    {
        var validator = new Mock<IValidator<OrderDiscountRequest>>();
        validator
            .Setup(item => item.ValidateAsync(It.IsAny<OrderDiscountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Products", "Pedido invalido.")]));

        var calculator = new OrderDiscountCalculator(validator.Object);

        var act = () => calculator.ExecuteAsync(Request([], Rules()));

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Pedido invalido*");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Not_Apply_Discount_When_No_Rule_Matches()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [Product(ProductsTypeEnum.Fries, 10m)],
            Rules()));

        result.SubTotalValue.Should().Be(10m);
        result.DiscountPercentage.Should().Be(0m);
        result.DiscountValue.Should().Be(0m);
        result.TotalValue.Should().Be(10m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Calculate_Subtotal_With_Product_Prices()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, 10m),
                Product(ProductsTypeEnum.Fries, 5m),
                Product(ProductsTypeEnum.Refreshment, 3m)
            ],
            Rules()));

        result.SubTotalValue.Should().Be(18m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Apply_20_Percent_When_Order_Has_Sandwich_Fries_And_Refreshment()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Fries, 10m),
                Product(ProductsTypeEnum.Refreshment, 5m)
            ],
            Rules()));

        result.SubTotalValue.Should().Be(35m);
        result.DiscountPercentage.Should().Be(20m);
        result.DiscountValue.Should().Be(7m);
        result.TotalValue.Should().Be(28m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Apply_15_Percent_When_Order_Has_Sandwich_And_Refreshment()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Refreshment, 10m)
            ],
            Rules()));

        result.SubTotalValue.Should().Be(30m);
        result.DiscountPercentage.Should().Be(15m);
        result.DiscountValue.Should().Be(4.50m);
        result.TotalValue.Should().Be(25.50m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Apply_10_Percent_When_Order_Has_Sandwich_And_Fries()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Fries, 10m)
            ],
            Rules()));

        result.SubTotalValue.Should().Be(30m);
        result.DiscountPercentage.Should().Be(10m);
        result.DiscountValue.Should().Be(3m);
        result.TotalValue.Should().Be(27m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Choose_Highest_Discount_When_More_Than_One_Rule_Matches()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Refreshment, 10m)
            ],
            [
                Rule(true, false, true, 10m),
                Rule(true, false, true, 15m)
            ]));

        result.DiscountPercentage.Should().Be(15m);
        result.DiscountValue.Should().Be(4.50m);
        result.TotalValue.Should().Be(25.50m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Ignore_Inactive_Rules()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Refreshment, 10m)
            ],
            [Rule(true, false, true, 15m, isActive: false)]));

        result.DiscountPercentage.Should().Be(0m);
        result.DiscountValue.Should().Be(0m);
        result.TotalValue.Should().Be(30m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Round_Discount_Away_From_Zero_With_Two_Decimal_Places()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [Product(ProductsTypeEnum.Sandwich, 10.05m)],
            [Rule(true, false, false, 10m)]));

        result.DiscountValue.Should().Be(1.01m);
        result.TotalValue.Should().Be(9.04m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Calculate_Total_As_Subtotal_Minus_DiscountValue()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, 40m),
                Product(ProductsTypeEnum.Refreshment, 10m)
            ],
            Rules()));

        result.TotalValue.Should().Be(result.SubTotalValue - result.DiscountValue);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Use_Zero_When_Product_Price_Is_Null()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, null),
                Product(ProductsTypeEnum.Refreshment, 10m)
            ],
            Rules()));

        result.SubTotalValue.Should().Be(10m);
        result.DiscountPercentage.Should().Be(15m);
        result.DiscountValue.Should().Be(1.50m);
        result.TotalValue.Should().Be(8.50m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Ignore_Null_Product_Type_When_Matching_Rule()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(null, 8m),
                Product(ProductsTypeEnum.Sandwich, 20m)
            ],
            [Rule(true, false, false, 10m)]));

        result.SubTotalValue.Should().Be(28m);
        result.DiscountPercentage.Should().Be(10m);
        result.DiscountValue.Should().Be(2.80m);
        result.TotalValue.Should().Be(25.20m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Not_Use_Default_Product_Type_When_Matching_Rule()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Default, 8m),
                Product(ProductsTypeEnum.Sandwich, 20m)
            ],
            [Rule(true, false, false, 10m)]));

        result.SubTotalValue.Should().Be(28m);
        result.DiscountPercentage.Should().Be(10m);
        result.DiscountValue.Should().Be(2.80m);
        result.TotalValue.Should().Be(25.20m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Not_Apply_Rule_When_Product_Combination_Does_Not_Match_Exactly()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Fries, 10m),
                Product(ProductsTypeEnum.Refreshment, 5m)
            ],
            [Rule(true, true, false, 10m)]));

        result.DiscountPercentage.Should().Be(0m);
        result.DiscountValue.Should().Be(0m);
        result.TotalValue.Should().Be(35m);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Calculated_Result_Without_PostProcess_Changes()
    {
        var result = await Calculator().ExecuteAsync(Request(
            [
                Product(ProductsTypeEnum.Sandwich, 20m),
                Product(ProductsTypeEnum.Refreshment, 10m)
            ],
            Rules()));

        result.Should().BeEquivalentTo(new OrderDiscountResult
        {
            SubTotalValue = 30m,
            DiscountPercentage = 15m,
            DiscountValue = 4.50m,
            TotalValue = 25.50m
        });
    }

    private static OrderDiscountCalculator Calculator()
    {
        return new OrderDiscountCalculator(new OrderDiscountValidator());
    }

    private static OrderDiscountRequest Request(
        IReadOnlyCollection<ProductEntity> products,
        IReadOnlyCollection<OrderDiscountRuleEntity> rules)
    {
        return new OrderDiscountRequest
        {
            Products = products,
            Rules = rules
        };
    }

    private static ProductEntity Product(ProductsTypeEnum? type, decimal? price)
    {
        return new ProductEntity
        {
            Id = (long)(type ?? ProductsTypeEnum.Default),
            Name = type?.ToString(),
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
        decimal discountPercentage,
        bool isActive = true)
    {
        return new OrderDiscountRuleEntity
        {
            HasSandwich = hasSandwich,
            HasFries = hasFries,
            HasRefreshment = hasRefreshment,
            DiscountPercentage = discountPercentage,
            IsActive = isActive,
            IsDeleted = false
        };
    }
}
