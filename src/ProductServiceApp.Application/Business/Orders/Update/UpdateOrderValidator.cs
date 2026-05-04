using FluentValidation;
using ProductServiceApp.Domain.Services.Orders.Handlers;

namespace ProductServiceApp.Application.Business.Orders.Update;

public class UpdateOrderValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotNull().GreaterThan(0).WithMessage("Id invalido.");

        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("Informe pelo menos um produto no pedido.");

        RuleForEach(x => x.Products)
            .ChildRules(product =>
            {
                product.RuleFor(x => x.ProductId)
                    .GreaterThan(0).WithMessage("Produto invalido no pedido.");

                product.RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("A quantidade do produto deve ser maior que zero.");
            });

        RuleFor(x => x.Products)
            .Must(products => products.Select(product => product.ProductId).Distinct().Count() == products.Count)
            .WithMessage("O pedido nao pode conter produtos repetidos. Informe a quantidade no campo quantity.");

    }
}
