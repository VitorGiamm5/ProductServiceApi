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

namespace ProductServiceApp.Application.Business.Orders.Update;

public class UpdateOrderBusiness(
        IOrderCommandRepository repository,
        IOrderQueryRepository readRepository,
        IProductQueryRepository<ProductEntity> productRepository,
        IOrderDiscountRuleQueryRepository discountRuleRepository,
        IOrderDiscountCalculator calculator,
        IValidator<UpdateOrderCommand> validator)
    : BaseBusinessService<UpdateOrderCommand, OrderDraft, OrderEntity, OrderResponse>,
    IUpdateOrderBusiness
{
    protected override async Task<OrderDraft> PreProcessAsync(UpdateOrderCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        await readRepository.GetByIdAsync(input.Id!.Value, ct);

        var products = await LoadProductsAsync(input.ProductIds, ct);
        var rules = (await discountRuleRepository.GetActiveAsync(ct)).ToList();

        calculator.Calculate(products, rules);

        return new OrderDraft
        {
            Id = input.Id.Value,
            Products = products,
            DiscountRules = rules,
            IsActive = input.IsActive,
            IsDeleted = input.IsDeleted
        };
    }

    protected override async Task<OrderEntity> ProcessAsync(OrderDraft input, CancellationToken ct)
    {
        var result = calculator.Calculate(input.Products, input.DiscountRules);

        var entity = new OrderEntity
        {
            Id = input.Id,
            UpdatedDate = DateTime.UtcNow,
            UpdatedByUserId = 0,
            IsActive = input.IsActive,
            IsDeleted = input.IsDeleted ?? false,
            SubTotalValue = result.SubTotalValue,
            TotalValue = result.TotalValue,
            DiscountPercentage = result.DiscountPercentage,
            DiscountValue = result.DiscountValue,
            OrderProducts = [.. input.Products.Select(product => new OrderProductEntity
            {
                Id = input.Id,
                ProductId = product.Id,
                UnitPrice = product.Price.GetValueOrDefault(),
                Product = product
            })]
        };

        return await repository.UpdateAsync(entity, input.Id, ct);
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
