using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.DateTimes;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Products.Business;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Business.Products.Update;

public sealed record UpdateProductToProcess(
    UpdateProductCommand Input,
    DateTime UpdatedDate);

public sealed record UpdateProductToPostProcess(
    ProductEntity UpdatedProduct);

public class UpdateProductBusiness(
        IProductCommandRepository<ProductEntity> repository,
        IProductCacheService cache,
        IValidator<UpdateProductCommand> validator
    )
    : BaseBusinessService<UpdateProductCommand, UpdateProductToProcess, UpdateProductToPostProcess, ProductResponse>(),
    IUpdateProductBusiness
{
    #region INBOX

    protected override async Task<UpdateProductToProcess> PreProcessAsync(
        UpdateProductCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        return new UpdateProductToProcess(input, DateTimeProvider.UtcNowAsUnspecified());
    }

    #endregion

    #region PROCESS

    protected override async Task<UpdateProductToPostProcess> ProcessAsync(
        UpdateProductToProcess input, CancellationToken ct)
    {
        var entity = MapToProcess(input);
        var result = await repository.UpdateAsync(entity, entity.Id, ct);

        return MapToPostProcess(result);
    }

    #endregion

    #region OUTBOX

    protected override async Task<ProductResponse> PostProcessAsync(
        UpdateProductToPostProcess result, CancellationToken ct)
    {
        await cache.InvalidateAllAsync(ct);
        await cache.SetByIdAsync(result.UpdatedProduct, ct);

        return new ProductResponse(result.UpdatedProduct);
    }

    #endregion

    #region MAP

    public static ProductEntity MapToProcess(UpdateProductToProcess intermediate)
    {
        var entity = intermediate.Input.MapTo();

        entity.UpdatedDate = intermediate.UpdatedDate;
        entity.IsDeleted = intermediate.Input.IsDeleted;
        entity.UpdatedByUserId = 0;

        return entity;
    }

    public static UpdateProductToPostProcess MapToPostProcess(ProductEntity updatedProduct)
    {
        return new UpdateProductToPostProcess(updatedProduct);
    }

    #endregion
}
