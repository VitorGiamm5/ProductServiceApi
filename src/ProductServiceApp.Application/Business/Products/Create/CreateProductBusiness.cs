using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.DateTimes;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Products.Business;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Business.Products.Create;

public sealed record CreateProductToProcess(
    CreateProductCommand Input,
    DateTime CreatedDate);

public sealed record CreateProductToPostProcess(
    ProductEntity CreatedProduct);

public class CreateProductBusiness(
        IProductCommandRepository<ProductEntity> repository,
        IProductCacheService cache,
        IValidator<CreateProductCommand> validator)
    : BaseBusinessService<CreateProductCommand, CreateProductToProcess, CreateProductToPostProcess, ProductResponse>,
    ICreateProductBusiness
{
    #region INBOX

    protected override async Task<CreateProductToProcess> PreProcessAsync(
        CreateProductCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        return new CreateProductToProcess(input, DateTimeProvider.UtcNowAsUnspecified());
    }

    #endregion

    #region PROCESS

    protected override async Task<CreateProductToPostProcess> ProcessAsync(
        CreateProductToProcess input, CancellationToken ct)
    {
        var entity = MapToProcess(input);
        var result = await repository.CreateAsync(entity, ct);

        return MapToPostProcess(result);
    }

    #endregion

    #region OUTBOX

    protected override async Task<ProductResponse> PostProcessAsync(
        CreateProductToPostProcess result, CancellationToken ct)
    {
        await cache.InvalidateAllAsync(ct);
        await cache.SetByIdAsync(result.CreatedProduct, ct);

        return new ProductResponse(result.CreatedProduct);
    }

    #endregion

    #region MAP

    public static ProductEntity MapToProcess(CreateProductToProcess intermediate)
    {
        var entity = intermediate.Input.MapTo();

        entity.CreatedDate = intermediate.CreatedDate;
        entity.IsDeleted = false;
        entity.CreatedByUserId = 0;

        return entity;
    }

    public static CreateProductToPostProcess MapToPostProcess(ProductEntity createdProduct)
    {
        return new CreateProductToPostProcess(createdProduct);
    }

    #endregion
}
