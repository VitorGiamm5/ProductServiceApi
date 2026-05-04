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

namespace ProductServiceApp.Application.Business.Orders.Update;

public class UpdateOrderBusiness(
        IOrderCommandRepository repository,
        IOrderQueryRepository readRepository,
        LoadProductsAsync loadProductsAsync,
        IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity> discountRuleRepository,
        IOrderDiscountCalculator calculator,
        IValidator<UpdateOrderCommand> validator)
    : BaseBusinessService<UpdateOrderCommand, OrderEntity, OrderEntity, OrderResponse>,
    IUpdateOrderBusiness
{
    //INBOX
    protected override async Task<OrderEntity> PreProcessAsync(UpdateOrderCommand input, CancellationToken ct)
    {
        #region VALIDATION

        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        #endregion

        #region Data Load

        await readRepository.GetByIdAsync(input.Id!.Value, ct);

        var products = await loadProductsAsync.ExecuteAsync(input.ProductIds, ct);
        var rules = (await discountRuleRepository.GetActiveAsync(ct)).ToList();

        #endregion

        #region Additional Features - Order Discount Calculation

        var orderCalculated = await calculator.ExecuteAsync(new OrderDiscountRequest
        {
            Products = products.ToFrozenSet(),
            Rules = rules.ToFrozenSet()
        }, ct);

        #endregion

        #region Entity Mapping

        return new OrderEntity
        {
            Id = input.Id ?? 0L,
            UpdatedDate = DateTime.UtcNow,
            UpdatedByUserId = 0,
            IsActive = input.IsActive,
            IsDeleted = input.IsDeleted ?? false,
            SubTotalValue = orderCalculated.SubTotalValue,
            TotalValue = orderCalculated.TotalValue,
            DiscountPercentage = orderCalculated.DiscountPercentage,
            DiscountValue = orderCalculated.DiscountValue,
            OrderProducts = [.. products
                .Select(product => new OrderProductEntity
                {
                    Id = input.Id ?? 0L,
                    ProductId = product.Id,
                    UnitPrice = product.Price.GetValueOrDefault(),
                    Product = product
                })]
        };

        #endregion
    }

    //PROCESS
    protected override async Task<OrderEntity> ProcessAsync(OrderEntity input, CancellationToken ct)
    {
        return await repository.UpdateAsync(input, input.Id, ct);
    }

    //OUTBOX
    protected override Task<OrderResponse> PostProcessAsync(OrderEntity result, CancellationToken ct)
    {
        return Task.FromResult(new OrderResponse(result));
    }
}
