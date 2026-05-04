using FluentValidation;
using ProductServiceApp.Application.Extensions.AssertionsValidator.Products;
using ProductServiceApp.Domain.Business.Orders.AdditionalFeaturesBusiness.OrderDiscount;

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
            .NoDuplicateProductsType();
    }
}
