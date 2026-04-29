using FluentValidation;
using ProductServiceApp.Domain.Business.Orders.Handlers;

namespace ProductServiceApp.Application.Business.Orders.Create;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ProductIds)
            .NotEmpty().WithMessage("Informe pelo menos um produto no pedido.");

        RuleForEach(x => x.ProductIds)
            .GreaterThan(0).WithMessage("Produto invalido no pedido.");
    }
}
