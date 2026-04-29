using FluentValidation;
using ProductServiceApp.Domain.Business.Products.Handlers;

namespace ProductServiceApp.Application.Business.Products.Create;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de produto inválido.");
    }
}
