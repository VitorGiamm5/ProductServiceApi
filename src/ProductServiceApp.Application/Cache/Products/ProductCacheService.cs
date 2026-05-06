using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductServiceApp.Application.Cache.Redis;
using ProductServiceApp.Domain.Entities.Products;
using System.Text.Json;

namespace ProductServiceApp.Application.Cache.Products;

public class ProductCacheService(
    IRedisCacheClient cache,
    IOptions<RedisCacheOptions> options,
    ILogger<ProductCacheService> logger)
    : IProductCacheService
{
    private const string Feature = "products";
    private const string AllProductsKey = "products:all";
    private const string ProductByIdKeyPrefix = "products:id:";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RedisCacheEntryOptions _cacheOptions = new(
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(options.Value.ProductsAbsoluteExpirationMinutes),
        SlidingExpiration: TimeSpan.FromMinutes(options.Value.ProductsSlidingExpirationMinutes));

    public async Task<ProductEntity[]?> GetAllAsync(CancellationToken cancellationToken)
    {
        return await TryGetAsync<ProductEntity[]>("get_all", AllProductsKey, cancellationToken);
    }

    public async Task SetAllAsync(IEnumerable<ProductEntity> products, CancellationToken cancellationToken)
    {
        await TrySetAsync("set_all", AllProductsKey, products.ToArray(), cancellationToken);
    }

    public async Task<ProductEntity?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await TryGetAsync<ProductEntity>("get_by_id", ProductByIdKey(id), cancellationToken);
    }

    public async Task SetByIdAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        if (product.Id <= 0)
            return;

        await TrySetAsync("set_by_id", ProductByIdKey(product.Id), product, cancellationToken);
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken)
    {
        await TryRemoveAsync("invalidate_all", AllProductsKey, cancellationToken);
    }

    public async Task InvalidateByIdAsync(long id, CancellationToken cancellationToken)
    {
        await TryRemoveAsync("invalidate_by_id", ProductByIdKey(id), cancellationToken);
    }

    private static string ProductByIdKey(long id) => $"{ProductByIdKeyPrefix}{id}";

    private async Task<T?> TryGetAsync<T>(string operation, string key, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await cache.GetStringAsync(Feature, operation, key, cancellationToken);
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

    private async Task TrySetAsync<T>(string operation, string key, T value, CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonSerializer.Serialize(value, JsonOptions);
            await cache.SetStringAsync(Feature, operation, key, payload, _cacheOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache write failed for key {CacheKey}.", key);
        }
    }

    private async Task TryRemoveAsync(string operation, string key, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(Feature, operation, key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache invalidation failed for key {CacheKey}.", key);
        }
    }
}
