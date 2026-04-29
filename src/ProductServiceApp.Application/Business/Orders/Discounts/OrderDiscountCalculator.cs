using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;

namespace ProductServiceApp.Application.Business.Orders.Discounts;

public interface IOrderDiscountCalculator
{
    OrderDiscountResult Calculate(
        IReadOnlyCollection<ProductEntity> products,
        IReadOnlyCollection<OrderDiscountRuleEntity> rules);
}

public sealed class OrderDiscountCalculator : IOrderDiscountCalculator
{
    public OrderDiscountResult Calculate(
        IReadOnlyCollection<ProductEntity> products,
        IReadOnlyCollection<OrderDiscountRuleEntity> rules)
    {
        ValidateProducts(products);

        var subTotal = products.Sum(product => product.Price.GetValueOrDefault());
        var rule = FindBestRule(products, rules);
        var discountPercentage = rule?.DiscountPercentage ?? decimal.Zero;
        var discountValue = decimal.Round(subTotal * discountPercentage / 100m, 2, MidpointRounding.AwayFromZero);
        var total = subTotal - discountValue;

        return new OrderDiscountResult
        {
            SubTotalValue = subTotal,
            DiscountPercentage = discountPercentage,
            DiscountValue = discountValue,
            TotalValue = total
        };
    }

    private static void ValidateProducts(IReadOnlyCollection<ProductEntity> products)
    {
        if (products.Count == 0)
        {
            throw new ArgumentException("O pedido deve ter pelo menos um produto.");
        }

        var duplicatedType = products
            .Where(product => product.Type is not null and not ProductsTypeEnum.Default)
            .GroupBy(product => product.Type!.Value)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicatedType is not null)
        {
            throw new ArgumentException($"O pedido nao pode conter produtos duplicados do tipo {GetTypeName(duplicatedType.Key)}.");
        }
    }

    private static OrderDiscountRuleEntity? FindBestRule(
        IReadOnlyCollection<ProductEntity> products,
        IReadOnlyCollection<OrderDiscountRuleEntity> rules)
    {
        var types = products
            .Where(product => product.Type is not null)
            .Select(product => product.Type!.Value)
            .ToHashSet();

        var hasSandwich = types.Contains(ProductsTypeEnum.Sandwich);
        var hasFries = types.Contains(ProductsTypeEnum.Fries);
        var hasRefreshment = types.Contains(ProductsTypeEnum.Refreshment);

        return rules
            .Where(rule =>
                rule.IsActive != false &&
                rule.HasSandwich == hasSandwich &&
                rule.HasFries == hasFries &&
                rule.HasRefreshment == hasRefreshment)
            .OrderByDescending(rule => rule.DiscountPercentage)
            .FirstOrDefault();
    }

    private static string GetTypeName(ProductsTypeEnum type) => type switch
    {
        ProductsTypeEnum.Sandwich => "Sanduiche",
        ProductsTypeEnum.Fries => "Batata",
        ProductsTypeEnum.Refreshment => "Refrigerante",
        _ => "Nao definido"
    };
}
