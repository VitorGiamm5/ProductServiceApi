using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Orders.AdditionalFeaturesBusiness.OrderDiscount;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Enums.Products;

namespace ProductServiceApp.Application.Business.Orders.OrderDiscount;

public class OrderDiscountCalculator(
        IValidator<OrderDiscountRequest> validator)
    : BaseBusinessService<OrderDiscountRequest, OrderDiscountRequest, OrderDiscountResult, OrderDiscountResult>,
    IOrderDiscountCalculator
{
    private readonly IValidator<OrderDiscountRequest> _validator = validator;


    // INBOX
    protected override async Task<OrderDiscountRequest> PreProcessAsync(OrderDiscountRequest input, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        return input;
    }

    // PROCESS
    protected override async Task<OrderDiscountResult> ProcessAsync(OrderDiscountRequest input, CancellationToken ct)
    {
        var subTotal = input.Products.Sum(product => product.Price.GetValueOrDefault());
        var rule = FindBestRule(input.Products, input.Rules);
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

    // OUTBOX
    protected override Task<OrderDiscountResult> PostProcessAsync(OrderDiscountResult result, CancellationToken ct)
    {
        return Task.FromResult(result);
    }

    #region Private Methods

    /// <summary>
    /// Finds the best applicable discount rule based on the product types in the order.
    /// Filters active rules that exactly match the combination of present product types
    /// (sandwich, fries, refreshment) and returns the one with the highest discount percentage.
    /// </summary>
    /// <param name="products">Collection of order products used to identify the present types.</param>
    /// <param name="rules">Collection of active discount rules to be evaluated.</param>
    /// <returns>The discount rule with the highest applicable percentage for the order, or <c>null</c> if no compatible rule is found.</returns>
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

    #endregion

}
