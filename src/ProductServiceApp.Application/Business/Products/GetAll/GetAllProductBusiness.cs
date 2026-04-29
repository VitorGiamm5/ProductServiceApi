using ProductServiceApp.Application.Business.Base;
using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Business.Products.Business;
using ProductServiceApp.Domain.Business.Products.Dtos;
using ProductServiceApp.Domain.Business.Products.Handlers;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Business.Products.GetAll;

public class GetAllProductBusiness(
        IProductQueryRepository<ProductEntity> repository,
        IProductCacheService cache
    )
    : BaseBusinessService<GetAllProductQuery, GetAllProductQuery, IEnumerable<ProductEntity>, IEnumerable<ProductResponse>>,
    IGetAllProductBusiness
{
    protected override Task<GetAllProductQuery> PreProcessAsync(
        GetAllProductQuery input, CancellationToken ct)
    {
        return Task.FromResult(input);
    }

    protected override async Task<IEnumerable<ProductEntity>> ProcessAsync(
        GetAllProductQuery input, CancellationToken ct)
    {
        var cachedProducts = await cache.GetAllAsync(ct);
        if (cachedProducts is not null)
            return cachedProducts;

        var products = (await repository.GetAllAsync(ct)).ToArray();
        await cache.SetAllAsync(products, ct);

        foreach (var product in products)
        {
            await cache.SetByIdAsync(product, ct);
        }

        return products;
    }

    protected override Task<IEnumerable<ProductResponse>> PostProcessAsync(
        IEnumerable<ProductEntity> result, CancellationToken ct)
    {
        return Task.FromResult(result.Select(x => new ProductResponse(x)));
    }
}
