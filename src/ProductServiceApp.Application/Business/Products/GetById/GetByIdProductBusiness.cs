using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products.GetById;

public class GetByIdProductBusiness(
        IProductQueryRepository<ProductEntity> repository,
        IProductCacheService cache,
        IValidator<GetByIdProductQuery> validator
    )
    : BaseBusinessService<GetByIdProductQuery, GetByIdProductQuery, ProductEntity, ProductResponse>,
      IGetByIdProductBusiness
{
    protected override async Task<GetByIdProductQuery> PreProcessAsync(
        GetByIdProductQuery input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        return input;
    }

    protected override async Task<ProductEntity> ProcessAsync(
        GetByIdProductQuery input, CancellationToken ct)
    {
        var cachedProduct = await cache.GetByIdAsync(input.Id, ct);
        if (cachedProduct is not null)
            return cachedProduct;

        var result = await repository.GetByIdAsync(input.Id, ct);
        if (result is null)
            throw new KeyNotFoundException($"Produto {input.Id} não encontrado.");

        await cache.SetByIdAsync(result, ct);

        return result;
    }

    protected override async Task<ProductResponse> PostProcessAsync(
        ProductEntity result, CancellationToken ct)
    {
        return await Task.FromResult(new ProductResponse(result));
    }
}
