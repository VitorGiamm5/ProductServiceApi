using FluentValidation;
using ProductServiceApp.Domain.Business.Orders.Handlers;

namespace ProductServiceApp.Application.Business.Orders.Delete;

public class DeleteOrderValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id invalido.");
    }
}
