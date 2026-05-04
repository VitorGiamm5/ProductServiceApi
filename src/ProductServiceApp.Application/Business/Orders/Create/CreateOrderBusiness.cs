using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Domain.Business.Orders.AdditionalFeaturesBusiness.OrderDiscount;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Repositories.Products;
using System.Collections.Frozen;

namespace ProductServiceApp.Application.Business.Orders.Create;

public class CreateOrderBusiness(
        IOrderCommandRepository repository,
        IProductQueryRepository<ProductEntity> productRepository,
        IOrderDiscountRuleQueryRepository<OrderDiscountRuleEntity> discountRuleRepository,
        IOrderDiscountCalculator calculator,
        IValidator<CreateOrderCommand> validator)
    : BaseBusinessService<CreateOrderCommand, OrderEntity, OrderEntity, OrderResponse>,
    ICreateOrderBusiness
{
    //INBOX
    protected override async Task<OrderEntity> PreProcessAsync(CreateOrderCommand input, CancellationToken ct)
    {
        #region VALIDATION

        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        #endregion

        #region Data Load

        var products = await ExecuteAsync(input.ProductIds, ct);
        var rules = await discountRuleRepository.GetActiveAsync(ct);
        var now = DateTime.UtcNow;

        #endregion

        #region Additional Features - Order Discount Calculation

        var orderCalculated = await calculator.ExecuteAsync(new OrderDiscountRequest
        {
            Products = [.. products.ToFrozenSet()],
            Rules = [.. rules.ToFrozenSet()],
        }, ct);

        #endregion

        #region Entity Mapping

        return new OrderEntity
        {
            CreatedDate = now,
            CreatedByUserId = 0,
            IsActive = input.IsActive,
            IsDeleted = input.IsDeleted,
            SubTotalValue = orderCalculated.SubTotalValue,
            TotalValue = orderCalculated.TotalValue,
            DiscountPercentage = orderCalculated.DiscountPercentage,
            DiscountValue = orderCalculated.DiscountValue,
            OrdersAudit = new OrderAuditEntity
            {
                CreatedDate = now,
                CreatedByUserId = 0,
                IsActive = true,
                IsDeleted = false
            },
            OrderProducts = [.. products
                .Select(product => new OrderProductEntity
                {
                    ProductId = product.Id,
                    UnitPrice = product.Price.GetValueOrDefault(),
                    Product = product
                })
            ]
        };

        #endregion
    }

    //PROCESS
    protected override async Task<OrderEntity> ProcessAsync(OrderEntity input, CancellationToken ct)
    {
        return await repository.CreateAsync(input, ct);
    }

    //OUTBOX
    protected override Task<OrderResponse> PostProcessAsync(OrderEntity result, CancellationToken ct)
    {
        return Task.FromResult(new OrderResponse(result));
    }

    #region - Helpers

    private async Task<List<ProductEntity>> ExecuteAsync(IEnumerable<long> productIds, CancellationToken ct)
    {
        var ids = productIds.Distinct().ToHashSet();
        var products = (await productRepository.GetAllAsync(ct))
            .Where(product => ids.Contains(product.Id))
            .ToList();

        if (products.Count != ids.Count)
        {
            throw new ArgumentException("Um ou mais produtos informados no pedido nao foram encontrados.");
        }

        return products;
    }

    #endregion
}
