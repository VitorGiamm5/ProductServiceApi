using FluentValidation;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Business.Products.GetById;

public class GetByIdProductValidator : AbstractValidator<GetByIdProductQuery>
{
    public GetByIdProductValidator(IProductQueryRepository<ProductEntity> repository)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id inválido.")
            .MustAsync(async (id, ct) =>
            {
                var entity = await repository.GetByIdAsync(id, ct);
                return entity is not null;
            })
            .When(x => x.Id > 0)
            .WithMessage(x => $"Produto {x.Id} não encontrado.");
    }
}
