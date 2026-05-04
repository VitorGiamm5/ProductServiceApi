using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Business.Products.GetByIdList;
using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Domain.DateTimes;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Services.Orders.AdditionalFeaturesBusiness.OrderDiscount;
using ProductServiceApp.Domain.Services.Orders.Business;
using ProductServiceApp.Domain.Services.Orders.Dtos;
using ProductServiceApp.Domain.Services.Orders.Handlers;
using System.Collections.Frozen;

namespace ProductServiceApp.Application.Business.Orders.Update;

public sealed record UpdateOrderToProcess(
    UpdateOrderCommand Input,
    IReadOnlyCollection<OrderDiscountProduct> Products,
    OrderDiscountResult OrderCalculated,
    DateTime UpdatedDate);

public sealed record UpdateOrderToPostProcess(
    OrderEntity UpdatedOrder);

public class UpdateOrderBusiness(
        IOrderCommandRepository repository,
        IOrderQueryRepository readRepository,
        IOrderCacheService cache,
        LoadProductsAsync loadProductsAsync,
        IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity> discountRuleRepository,
        IOrderDiscountCalculator calculator,
        IValidator<UpdateOrderCommand> validator)
    : BaseBusinessService<UpdateOrderCommand, UpdateOrderToProcess, UpdateOrderToPostProcess, OrderResponse>,
    IUpdateOrderBusiness
{
    #region INBOX

    protected override async Task<UpdateOrderToProcess> PreProcessAsync(UpdateOrderCommand input, CancellationToken ct)
    {
        #region VALIDATION

        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        #endregion

        #region Data Load

        await readRepository.GetByIdAsync(input.Id!.Value, ct);

        var products = await loadProductsAsync.ExecuteAsync(input.Products.Select(item => item.ProductId), ct);
        var quantities = input.Products.ToDictionary(item => item.ProductId, item => item.Quantity);
        var orderProducts = products
            .Select(product => new OrderDiscountProduct(product, quantities[product.Id]))
            .ToArray();
        var rules = (await discountRuleRepository.GetActiveAsync(ct)).ToList();

        #endregion

        #region Additional Features - Order Discount Calculation

        var orderCalculated = await calculator.ExecuteAsync(new OrderDiscountRequest
        {
            Products = orderProducts.ToFrozenSet(),
            Rules = rules.ToFrozenSet()
        }, ct);

        #endregion

        #region Map

        return new UpdateOrderToProcess(input, orderProducts, orderCalculated, DateTimeProvider.UtcNowAsUnspecified());

        #endregion
    }

    #endregion

    #region PROCESS
    protected override async Task<UpdateOrderToPostProcess> ProcessAsync(UpdateOrderToProcess input, CancellationToken ct)
    {
        var entity = MapToIntermediate(input);

        var result = await repository.UpdateAsync(entity, entity.Id, ct);

        return new UpdateOrderToPostProcess(result);
    }

    #endregion

    #region OUTBOX

    protected override async Task<OrderResponse> PostProcessAsync(UpdateOrderToPostProcess result, CancellationToken ct)
    {
        await cache.InvalidateAllAsync(ct);
        await cache.SetByIdAsync(result.UpdatedOrder, ct);

        return new OrderResponse(result.UpdatedOrder);
    }

    #endregion

    #region MAP

    public static OrderEntity MapToIntermediate(UpdateOrderToProcess intermediate)
    {
        return new OrderEntity
        {
            Id = intermediate.Input.Id ?? 0L,
            UpdatedDate = intermediate.UpdatedDate,
            UpdatedByUserId = 0,
            IsActive = intermediate.Input.IsActive,
            IsDeleted = intermediate.Input.IsDeleted ?? false,
            SubTotalValue = intermediate.OrderCalculated.SubTotalValue,
            TotalValue = intermediate.OrderCalculated.TotalValue,
            DiscountPercentage = intermediate.OrderCalculated.DiscountPercentage,
            DiscountValue = intermediate.OrderCalculated.DiscountValue,
            OrderProducts = [.. intermediate.Products
                .Select(item => new OrderProductEntity
                {
                    Id = intermediate.Input.Id ?? 0L,
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price.GetValueOrDefault(),
                    Product = item.Product
                })]
        };
    }

    //MAP
    public static UpdateOrderToPostProcess MapToPostProcess(OrderEntity entity)
    {
        return new UpdateOrderToPostProcess(entity);
    }

#endregion

}
