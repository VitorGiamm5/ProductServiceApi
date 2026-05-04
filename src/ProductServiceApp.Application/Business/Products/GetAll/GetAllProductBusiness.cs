using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;
using ProductServiceApp.Domain.Services.Products.Business;
using ProductServiceApp.Domain.Services.Products.Dtos;
using ProductServiceApp.Domain.Services.Products.Handlers;

namespace ProductServiceApp.Application.Business.Products.GetAll;

public sealed record GetAllProductToProcess(
    GetAllProductQuery Input);

public sealed record GetAllProductToPostProcess(
    IEnumerable<ProductEntity> RetrievedProducts);

public class GetAllProductBusiness(
        IProductQueryRepository<ProductEntity> repository,
        IProductCacheService cache
    )
    : BaseBusinessService<GetAllProductQuery, GetAllProductToProcess, GetAllProductToPostProcess, IEnumerable<ProductResponse>>,
    IGetAllProductBusiness
{
    #region INBOX

    protected override Task<GetAllProductToProcess> PreProcessAsync(
        GetAllProductQuery input, CancellationToken ct)
    {
        return Task.FromResult(new GetAllProductToProcess(input));
    }

    #endregion

    #region PROCESS

    protected override async Task<GetAllProductToPostProcess> ProcessAsync(
        GetAllProductToProcess input, CancellationToken ct)
    {
        var cachedProducts = await cache.GetAllAsync(ct);
        if (cachedProducts is not null)
            return MapToPostProcess(cachedProducts);

        var products = (await repository.GetAllAsync(ct)).ToArray();
        await cache.SetAllAsync(products, ct);

        foreach (var product in products)
        {
            await cache.SetByIdAsync(product, ct);
        }

        return MapToPostProcess(products);
    }

    #endregion

    #region OUTBOX

    protected override Task<IEnumerable<ProductResponse>> PostProcessAsync(
        GetAllProductToPostProcess result, CancellationToken ct)
    {
        return Task.FromResult(result.RetrievedProducts.Select(MapToResponse));
    }

    #endregion

    #region MAP

    public static GetAllProductToPostProcess MapToPostProcess(IEnumerable<ProductEntity> products)
    {
        return new GetAllProductToPostProcess(products);
    }

    public static ProductResponse MapToResponse(ProductEntity product)
    {
        return new ProductResponse(product);
    }

    #endregion
}
