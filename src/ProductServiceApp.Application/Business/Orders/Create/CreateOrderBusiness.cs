using System.Collections.Frozen;
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

namespace ProductServiceApp.Application.Business.Orders.Create;

public sealed record CreateOrderToProcess(
    CreateOrderCommand Input,
    IReadOnlyCollection<OrderDiscountProduct> Products,
    OrderDiscountResult OrderCalculated,
    DateTime CreatedDate);

public sealed record CreateOrderToPostProcess(
    OrderEntity CreatedOrder);

public class CreateOrderBusiness(
        LoadProductsAsync loadProductsAsync,
        IOrderCommandRepository repository,
        IOrderCacheService cache,
        IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity> discountRuleRepository,
        IOrderDiscountCalculator calculator,
        IValidator<CreateOrderCommand> validator)
    : BaseBusinessService<CreateOrderCommand, CreateOrderToProcess, CreateOrderToPostProcess, OrderResponse>,
    ICreateOrderBusiness
{
    #region INBOX

    protected override async Task<CreateOrderToProcess> PreProcessAsync(CreateOrderCommand input, CancellationToken ct)
    {
        #region VALIDATION

        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        #endregion

        #region Data Load

        var products = await loadProductsAsync.ExecuteAsync(input.Products.Select(item => item.ProductId), ct);
        var quantities = input.Products.ToDictionary(item => item.ProductId, item => item.Quantity);
        var orderProducts = products
            .Select(product => new OrderDiscountProduct(product, quantities[product.Id]))
            .ToArray();
        var rules = await discountRuleRepository.GetActiveAsync(ct);

        #endregion

        #region Order Discount Calculation

        var orderCalculated = await calculator.ExecuteAsync(new OrderDiscountRequest
        {
            Products = [.. orderProducts.ToFrozenSet()],
            Rules = [.. rules.ToFrozenSet()],
        }, ct);

        #endregion

        #region Map

        return new CreateOrderToProcess(input, orderProducts, orderCalculated, DateTimeProvider.UtcNowAsUnspecified());

        #endregion
    }

    #endregion

    #region PROCESS
    protected override async Task<CreateOrderToPostProcess> ProcessAsync(CreateOrderToProcess input, CancellationToken ct)
    {
        var entity = MapToProcess(input);

        var result = await repository.CreateAsync(entity, ct);

        return MapToPostProcess(result);
    }

    #endregion

    #region OUTBOX

    protected override async Task<OrderResponse> PostProcessAsync(CreateOrderToPostProcess result, CancellationToken ct)
    {
        await cache.InvalidateAllAsync(ct);
        await cache.SetByIdAsync(result.CreatedOrder, ct);

        return new OrderResponse(result.CreatedOrder);
    }

    #endregion

    #region MAP

    public static OrderEntity MapToProcess(CreateOrderToProcess intermediate)
    {
        return new OrderEntity
        {
            CreatedDate = intermediate.CreatedDate,
            CreatedByUserId = 0,
            IsActive = intermediate.Input.IsActive,
            IsDeleted = intermediate.Input.IsDeleted,
            SubTotalValue = intermediate.OrderCalculated.SubTotalValue,
            TotalValue = intermediate.OrderCalculated.TotalValue,
            DiscountPercentage = intermediate.OrderCalculated.DiscountPercentage,
            DiscountValue = intermediate.OrderCalculated.DiscountValue,
            OrdersAudit = new OrderAuditEntity
            {
                CreatedDate = intermediate.CreatedDate,
                CreatedByUserId = 0,
                IsActive = true,
                IsDeleted = false
            },
            OrderProducts = [.. intermediate.Products
                .Select(item => new OrderProductEntity
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price.GetValueOrDefault(),
                    Product = item.Product
                })
            ]
        };
    }

    public static CreateOrderToPostProcess MapToPostProcess(OrderEntity createdOrder)
    {
        return new CreateOrderToPostProcess(createdOrder);
    }

    #endregion
}
