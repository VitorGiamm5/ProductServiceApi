using FluentValidation;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Business.Products.Delete;

public class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator(IProductQueryRepository<ProductEntity> queryRepository)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id inválido.");

        RuleFor(x => x.Id)
            .MustAsync(async (id, ct) =>
            {
                var entity = await queryRepository.GetByIdAsync(id, ct);
                return entity is not null;
            }).WithMessage(x => $"Produto {x.Id} não encontrado.")
            .MustAsync(async (id, ct) =>
            {
                var entity = await queryRepository.GetByIdAsync(id, ct);
                return entity?.IsActive ?? false;
            }).WithMessage(x => $"Produto {x.Id} já foi deletado.");
    }
}
