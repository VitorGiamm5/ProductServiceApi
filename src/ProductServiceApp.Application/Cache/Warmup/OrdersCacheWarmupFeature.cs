using ProductServiceApp.Application.Cache.Orders;
using ProductServiceApp.Domain.Repositories.Orders;

namespace ProductServiceApp.Application.Cache.Warmup;

internal sealed class OrdersCacheWarmupFeature(
    IOrderQueryRepository repository,
    IOrderCacheService cache)
    : ICacheWarmupFeature
{
    public string FeatureName => "orders";

    public async Task<CacheWarmupFeatureResult> WarmupAsync(
        CacheWarmupFeatureOptions options,
        CancellationToken cancellationToken)
    {
        var orders = (await repository.GetAllAsync(cancellationToken)).ToArray();
        var itemsByIdWarmed = 0;

        if (options.WarmupAll)
            await cache.SetAllAsync(orders, cancellationToken);

        if (options.WarmupById)
        {
            foreach (var order in orders.Take(Math.Max(0, options.MaxItems)))
            {
                await cache.SetByIdAsync(order, cancellationToken);
                itemsByIdWarmed++;
            }
        }

        return new CacheWarmupFeatureResult(
            orders.Length,
            itemsByIdWarmed,
            options.WarmupAll,
            options.WarmupById);
    }
}
