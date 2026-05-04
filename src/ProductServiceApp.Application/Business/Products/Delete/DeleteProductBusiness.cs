using FluentValidation;
using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Base.Dtos;
using ProductServiceApp.Domain.Services.Products.Business;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Business.Products.Delete;

public sealed record DeleteProductToProcess(
    DeleteProductCommand Input,
    ProductEntity ProductToDelete);

public sealed record DeleteProductToPostProcess(
    ProductEntity DeletedProduct);

public class DeleteProductBusiness(
        IProductCommandRepository<ProductEntity> repository,
        IProductQueryRepository<ProductEntity> read,
        IProductCacheService cache,
        IValidator<DeleteProductCommand> validator
    )
    : BaseBusinessService<DeleteProductCommand, DeleteProductToProcess, DeleteProductToPostProcess, BooleanResponse>,
    IDeleteProductBusiness
{
    #region INBOX

    protected override async Task<DeleteProductToProcess> PreProcessAsync(
        DeleteProductCommand input, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var entity = await read.GetByIdAsync(input.Id, ct);

        return entity is null
            ? throw new KeyNotFoundException($"Produto {input.Id} nao encontrado.")
            : MapToProcess(new DeleteProductToProcess(input, entity));
    }

    #endregion

    #region PROCESS

    protected override async Task<DeleteProductToPostProcess> ProcessAsync(
        DeleteProductToProcess input, CancellationToken ct)
    {
        await repository.DeleteAsync(input.ProductToDelete.Id, ct);

        return MapToPostProcess(input.ProductToDelete);
    }

    #endregion

    #region OUTBOX

    protected override async Task<BooleanResponse> PostProcessAsync(
        DeleteProductToPostProcess result, CancellationToken ct)
    {
        await cache.InvalidateAllAsync(ct);
        await cache.InvalidateByIdAsync(result.DeletedProduct.Id, ct);

        return new BooleanResponse
        {
            IsSuccess = true
        };
    }

    #endregion

    #region MAP

    public static DeleteProductToProcess MapToProcess(DeleteProductToProcess intermediate)
    {
        return intermediate;
    }

    public static DeleteProductToPostProcess MapToPostProcess(ProductEntity deletedProduct)
    {
        return new DeleteProductToPostProcess(deletedProduct);
    }

    #endregion
}
