using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products.Create;

public class CreateProductBusiness(
        IProductCommandRepository<ProductEntity> repository,
        IProductCacheService cache,
        IValidator<CreateProductCommand> validator)
    : BaseBusinessService<CreateProductCommand, ProductEntity, ProductEntity, ProductResponse>,
    ICreateProductBusiness
{
    protected override async Task<ProductEntity> PreProcessAsync(
        CreateProductCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        ProductEntity entity = input.MapTo();

        entity.CreatedDate = DateTime.UtcNow;
        entity.IsDeleted = false;
        entity.CreatedByUserId = 0;

        return entity;
    }

    protected override async Task<ProductEntity> ProcessAsync(
        ProductEntity input, CancellationToken ct)
    {
        return await repository.CreateAsync(input, ct);
    }

    protected override async Task<ProductResponse> PostProcessAsync(
        ProductEntity result, CancellationToken ct)
    {
        await cache.InvalidateAllAsync(ct);
        await cache.SetByIdAsync(result, ct);

        return new ProductResponse(result);
    }
}
