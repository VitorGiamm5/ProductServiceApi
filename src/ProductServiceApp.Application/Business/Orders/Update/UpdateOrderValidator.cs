using FluentValidation;
using ProductServiceApp.Domain.Business.Orders.Handlers;

namespace ProductServiceApp.Application.Business.Orders.Update;

public class UpdateOrderValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotNull().GreaterThan(0).WithMessage("Id invalido.");

        RuleFor(x => x.ProductIds)
            .NotEmpty().WithMessage("Informe pelo menos um produto no pedido.");

        RuleForEach(x => x.ProductIds)
            .GreaterThan(0).WithMessage("Produto invalido no pedido.");
    }
}
