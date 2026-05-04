using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Business.Products.GetByIdList;
using ProductServiceApp.Domain.Business.Orders.AdditionalFeaturesBusiness.OrderDiscount;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Orders;
using System.Collections.Frozen;

namespace ProductServiceApp.Application.Business.Orders.Create;

/// <summary>
/// Intermediate object for the order creation process, containing the necessary data for order mapping and processing.
/// </summary>
/// <param name="Input">Order creation command containing the input data.</param>
/// <param name="Products">Collection of products loaded for the order.</param>
/// <param name="OrderCalculated">Result of the order discount calculation.</param>
/// <param name="CreatedDate">Order creation date.</param>
public sealed record CreateOrderIntermediate(
    CreateOrderCommand Input,
    IReadOnlyCollection<ProductEntity> Products,
    OrderDiscountResult OrderCalculated,
    DateTime CreatedDate);

public class CreateOrderBusiness(
        LoadProductsAsync loadProductsAsync,
        IOrderCommandRepository repository,
        IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity> discountRuleRepository,
        IOrderDiscountCalculator calculator,
        IValidator<CreateOrderCommand> validator)
    : BaseBusinessService<CreateOrderCommand, CreateOrderIntermediate, OrderEntity, OrderResponse>,
    ICreateOrderBusiness
{
    //INBOX
    protected override async Task<CreateOrderIntermediate> PreProcessAsync(CreateOrderCommand input, CancellationToken ct)
    {
        #region VALIDATION

        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        #endregion

        #region Data Load

        var products = await loadProductsAsync.ExecuteAsync(input.ProductIds, ct);
        var rules = await discountRuleRepository.GetActiveAsync(ct);

        #endregion

        #region Additional Features - Order Discount Calculation

        var orderCalculated = await calculator.ExecuteAsync(new OrderDiscountRequest
        {
            Products = [.. products.ToFrozenSet()],
            Rules = [.. rules.ToFrozenSet()],
        }, ct);

        #endregion

        #region Map

        return new CreateOrderIntermediate(input, products, orderCalculated, DateTime.UtcNow);

        #endregion
    }

    #region Entity Mapping

    //PROCESS
    protected override async Task<OrderEntity> ProcessAsync(CreateOrderIntermediate input, CancellationToken ct)
    {
        var entity = MapToIntermediate(input);

        return await repository.CreateAsync(entity, ct);
    }

    //OUTBOX
    protected override Task<OrderResponse> PostProcessAsync(OrderEntity result, CancellationToken ct)
    {
        return Task.FromResult(new OrderResponse(result));
    }

    //MAP
    public static OrderEntity MapToIntermediate(CreateOrderIntermediate intermediate)
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
                .Select(product => new OrderProductEntity
                {
                    ProductId = product.Id,
                    UnitPrice = product.Price.GetValueOrDefault(),
                    Product = product
                })
            ]
        };
    }

    #endregion
}
