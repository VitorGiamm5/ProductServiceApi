using FluentValidation;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Business.Products.Update;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id inválido.");
    }
}
