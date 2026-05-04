using FluentValidation;
using ProductServiceApp.Domain.Enums.Products;
using ProductServiceApp.Domain.Services.Orders.AdditionalFeaturesBusiness.OrderDiscount;

namespace ProductServiceApp.Application.Business.Orders.OrderDiscount;

public class OrderDiscountValidator : AbstractValidator<OrderDiscountRequest>
{
    public OrderDiscountValidator()
    {
        RuleFor(x => x)
            .NotNull().WithMessage("A requisição não pode ser nula.")
            .Must(request => request.Products != null && request.Products.Count > 0)
            .WithMessage("O pedido deve conter pelo menos um produto.");

        RuleFor(x => x.Products)
            .Must(products =>
            {
                var duplicatedType = products?
                    .Where(item => item.Product.Type is not null and not ProductsTypeEnum.Default)
                    .GroupBy(item => item.Product.Type!.Value)
                    .FirstOrDefault(group => group.Count() > 1);

                return duplicatedType is null;
            })
            .WithMessage("O pedido não pode conter produtos duplicados do mesmo tipo.");

        RuleForEach(x => x.Products)
            .Must(item => item.Quantity > 0)
            .WithMessage("A quantidade do produto deve ser maior que zero.");
    }
}
