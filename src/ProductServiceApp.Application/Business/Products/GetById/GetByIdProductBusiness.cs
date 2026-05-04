using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Products.Business;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Business.Products.GetById;

public sealed record GetByIdProductToProcess(
    GetByIdProductQuery Input);

public sealed record GetByIdProductToPostProcess(
    ProductEntity RetrievedProduct);

public class GetByIdProductBusiness(
        IProductQueryRepository<ProductEntity> repository,
        IProductCacheService cache,
        IValidator<GetByIdProductQuery> validator
    )
    : BaseBusinessService<GetByIdProductQuery, GetByIdProductToProcess, GetByIdProductToPostProcess, ProductResponse>,
      IGetByIdProductBusiness
{
    #region INBOX

    protected override async Task<GetByIdProductToProcess> PreProcessAsync(
        GetByIdProductQuery input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        return new GetByIdProductToProcess(input);
    }

    #endregion

    #region PROCESS

    protected override async Task<GetByIdProductToPostProcess> ProcessAsync(
        GetByIdProductToProcess input, CancellationToken ct)
    {
        var entity = MapToProcess(input);

        var cachedProduct = await cache.GetByIdAsync(entity.Id, ct);
        if (cachedProduct is not null)
            return MapToPostProcess(cachedProduct);

        var result = await repository.GetByIdAsync(entity.Id, ct);
        if (result is null)
            throw new KeyNotFoundException($"Produto {entity.Id} nao encontrado.");

        await cache.SetByIdAsync(result, ct);

        return MapToPostProcess(result);
    }

    #endregion

    #region OUTBOX

    protected override Task<ProductResponse> PostProcessAsync(
        GetByIdProductToPostProcess result, CancellationToken ct)
    {
        return Task.FromResult(MapToResponse(result));
    }

    #endregion

    #region MAP

    public static ProductEntity MapToProcess(GetByIdProductToProcess input)
    {
        return new ProductEntity
        {
            Id = input.Input.Id
        };
    }

    public static GetByIdProductToPostProcess MapToPostProcess(ProductEntity product)
    {
        return new GetByIdProductToPostProcess(product);
    }

    public static ProductResponse MapToResponse(GetByIdProductToPostProcess postProcess)
    {
        return new ProductResponse(postProcess.RetrievedProduct);
    }

    #endregion
}
