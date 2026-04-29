using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Business.Orders.Discounts;
using ProductServiceApp.Domain.Business.Orders.Business;
using ProductServiceApp.Domain.Business.Orders.Dtos;
using ProductServiceApp.Domain.Business.Orders.Handlers;
using ProductServiceApp.Domain.Entities.Orders;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Orders;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Orders.Create;

public class CreateOrderBusiness(
        IOrderCommandRepository repository,
        IProductQueryRepository<ProductEntity> productRepository,
        IOrderDiscountRuleQueryRepository discountRuleRepository,
        IOrderDiscountCalculator calculator,
        IValidator<CreateOrderCommand> validator)
    : BaseBusinessService<CreateOrderCommand, OrderDraft, OrderEntity, OrderResponse>,
    ICreateOrderBusiness
{
    protected override async Task<OrderDraft> PreProcessAsync(CreateOrderCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var products = await LoadProductsAsync(input.ProductIds, ct);
        var rules = (await discountRuleRepository.GetActiveAsync(ct)).ToList();

        calculator.Calculate(products, rules);

        return new OrderDraft
        {
            Products = products,
            DiscountRules = rules,
            IsActive = input.IsActive,
            IsDeleted = false
        };
    }

    protected override async Task<OrderEntity> ProcessAsync(OrderDraft input, CancellationToken ct)
    {
        var result = calculator.Calculate(input.Products, input.DiscountRules);
        var now = DateTime.UtcNow;

        var entity = new OrderEntity
        {
            CreatedDate = now,
            CreatedByUserId = 0,
            IsActive = input.IsActive,
            IsDeleted = false,
            SubTotalValue = result.SubTotalValue,
            TotalValue = result.TotalValue,
            DiscountPercentage = result.DiscountPercentage,
            DiscountValue = result.DiscountValue,
            OrdersAudit = new OrderAuditEntity
            {
                CreatedDate = now,
                CreatedByUserId = 0,
                IsActive = true,
                IsDeleted = false
            },
            OrderProducts = [.. input.Products.Select(product => new OrderProductEntity
            {
                ProductId = product.Id,
                UnitPrice = product.Price.GetValueOrDefault(),
                Product = product
            })]
        };

        return await repository.CreateAsync(entity, ct);
    }

    protected override Task<OrderResponse> PostProcessAsync(OrderEntity result, CancellationToken ct)
    {
        return Task.FromResult(new OrderResponse(result));
    }

    private async Task<List<ProductEntity>> LoadProductsAsync(IEnumerable<long> productIds, CancellationToken ct)
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
}
