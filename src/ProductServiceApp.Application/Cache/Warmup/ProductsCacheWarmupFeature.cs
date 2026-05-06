using ProductServiceApp.Application.Cache.Products;
using ProductServiceApp.Domain.Entities.Products;
using ProductServiceApp.Domain.Repositories.Products;

namespace ProductServiceApp.Application.Cache.Warmup;

internal sealed class ProductsCacheWarmupFeature(
    IProductQueryRepository<ProductEntity> repository,
    IProductCacheService cache)
    : ICacheWarmupFeature
{
    public string FeatureName => "products";

    public async Task<CacheWarmupFeatureResult> WarmupAsync(
        CacheWarmupFeatureOptions options,
        CancellationToken cancellationToken)
    {
        var products = (await repository.GetAllAsync(cancellationToken)).ToArray();
        var itemsByIdWarmed = 0;

        if (options.WarmupAll)
            await cache.SetAllAsync(products, cancellationToken);

        if (options.WarmupById)
        {
            foreach (var product in products.Take(Math.Max(0, options.MaxItems)))
            {
                await cache.SetByIdAsync(product, cancellationToken);
                itemsByIdWarmed++;
            }
        }

        return new CacheWarmupFeatureResult(
            products.Length,
            itemsByIdWarmed,
            options.WarmupAll,
            options.WarmupById);
    }
}
