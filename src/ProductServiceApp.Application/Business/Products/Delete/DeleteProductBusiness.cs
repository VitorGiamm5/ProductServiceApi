using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Business.Base.Dtos;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products.Delete;

public class DeleteProductBusiness(
        IProductCommandRepository<ProductEntity> repository,
        IProductQueryRepository<ProductEntity> read,
        IProductCacheService cache,
        IValidator<DeleteProductCommand> validator
    )
    : BaseBusinessService<DeleteProductCommand, ProductEntity, ProductEntity, BooleanResponse>,
    IDeleteProductBusiness
{
    protected override async Task<ProductEntity> PreProcessAsync(
        DeleteProductCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var entity = await read.GetByIdAsync(input.Id, ct);

        return entity is null
            ? throw new KeyNotFoundException($"Produto {input.Id} não encontrado.")
            : entity;
    }

    protected override async Task<ProductEntity> ProcessAsync(
        ProductEntity input, CancellationToken ct)
    {
        await repository.DeleteAsync(input.Id, ct);

        return input;
    }

    protected override async Task<BooleanResponse> PostProcessAsync(
        ProductEntity result, CancellationToken ct)
    {
        await cache.InvalidateAllAsync(ct);
        await cache.InvalidateByIdAsync(result.Id, ct);

        return new BooleanResponse
        {
            IsSuccess = true
        };
    }
}
