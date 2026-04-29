using FluentValidation;
using ProductServiceApp.Domain.Business.Orders.Handlers;

namespace ProductServiceApp.Application.Business.Orders.GetById;

public class GetByIdOrderValidator : AbstractValidator<GetByIdOrderQuery>
{
    public GetByIdOrderValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id invalido.");
    }
}
