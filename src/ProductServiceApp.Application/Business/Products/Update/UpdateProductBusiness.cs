using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products.Update;

public class UpdateProductBusiness(
        IProductCommandRepository<ProductEntity> repository,
        IProductCacheService cache,
        IValidator<UpdateProductCommand> validator
    )
    : BaseBusinessService<UpdateProductCommand, ProductEntity, ProductEntity, ProductResponse>(),
    IUpdateProductBusiness
{
    protected override async Task<ProductEntity> PreProcessAsync(
        UpdateProductCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        ProductEntity entity = input.MapTo();

        entity.UpdatedDate = DateTime.UtcNow;
        entity.IsDeleted = input.IsDeleted;
        entity.UpdatedByUserId = 0;

        return entity;
    }

    protected override async Task<ProductEntity> ProcessAsync(
        ProductEntity input, CancellationToken ct)
    {
        return await repository.UpdateAsync(input, input.Id, ct);
    }

    protected override async Task<ProductResponse> PostProcessAsync(
        ProductEntity result, CancellationToken ct)
    {
        await cache.InvalidateAllAsync(ct);
        await cache.SetByIdAsync(result, ct);

        return new ProductResponse(result);
    }
}
