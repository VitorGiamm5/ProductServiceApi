using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductServiceApp.Domain.Entities.Products;
using System.Text.Json;

namespace ProductServiceApp.Application.Cache.Products;

public class ProductCacheService(
    IDistributedCache cache,
    IConfiguration configuration,
    ILogger<ProductCacheService> logger)
    : IProductCacheService
{
    private const string AllProductsKey = "products:all";
    private const string ProductByIdKeyPrefix = "products:id:";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Redis:ProductsAbsoluteExpirationMinutes", 10)),
        SlidingExpiration = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Redis:ProductsSlidingExpirationMinutes", 2))
    };

    public async Task<ProductEntity[]?> GetAllAsync(CancellationToken cancellationToken)
    {
        return await TryGetAsync<ProductEntity[]>(AllProductsKey, cancellationToken);
    }

    public async Task SetAllAsync(IEnumerable<ProductEntity> products, CancellationToken cancellationToken)
    {
        await TrySetAsync(AllProductsKey, products.ToArray(), cancellationToken);
    }

    public async Task<ProductEntity?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await TryGetAsync<ProductEntity>(ProductByIdKey(id), cancellationToken);
    }

    public async Task SetByIdAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        if (product.Id <= 0)
            return;

        await TrySetAsync(ProductByIdKey(product.Id), product, cancellationToken);
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken)
    {
        await TryRemoveAsync(AllProductsKey, cancellationToken);
    }

    public async Task InvalidateByIdAsync(long id, CancellationToken cancellationToken)
    {
        await TryRemoveAsync(ProductByIdKey(id), cancellationToken);
    }

    private static string ProductByIdKey(long id) => $"{ProductByIdKeyPrefix}{id}";

    private async Task<T?> TryGetAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await cache.GetStringAsync(key, cancellationToken);
            return string.IsNullOrWhiteSpace(payload)
                ? default
                : JsonSerializer.Deserialize<T>(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache read failed for key {CacheKey}.", key);
            return default;
        }
    }

    private async Task TrySetAsync<T>(string key, T value, CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonSerializer.Serialize(value, JsonOptions);
            await cache.SetStringAsync(key, payload, _cacheOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache write failed for key {CacheKey}.", key);
        }
    }

    private async Task TryRemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache invalidation failed for key {CacheKey}.", key);
        }
    }
}
