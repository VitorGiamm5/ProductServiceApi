using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductServiceApp.Application.Cache.Redis;
using ProductServiceApp.Domain.Entities.Orders;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductServiceApp.Application.Cache.Orders;

public class OrderCacheService(
    IRedisCacheClient cache,
    IOptions<RedisCacheOptions> options,
    ILogger<OrderCacheService> logger)
    : IOrderCacheService
{
    private const string Feature = "orders";
    private const string AllOrdersKey = "orders:all";
    private const string OrderByIdKeyPrefix = "orders:id:";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private readonly RedisCacheEntryOptions _cacheOptions = new(
        AbsoluteExpirationRelativeToNow: TimeSpan.FromMinutes(options.Value.OrdersAbsoluteExpirationMinutes),
        SlidingExpiration: TimeSpan.FromMinutes(options.Value.OrdersSlidingExpirationMinutes));

    public async Task<OrderEntity[]?> GetAllAsync(CancellationToken cancellationToken)
    {
        return await TryGetAsync<OrderEntity[]>("get_all", AllOrdersKey, cancellationToken);
    }

    public async Task SetAllAsync(IEnumerable<OrderEntity> orders, CancellationToken cancellationToken)
    {
        await TrySetAsync("set_all", AllOrdersKey, orders.ToArray(), cancellationToken);
    }

    public async Task<OrderEntity?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await TryGetAsync<OrderEntity>("get_by_id", OrderByIdKey(id), cancellationToken);
    }

    public async Task SetByIdAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        if (order.Id <= 0)
            return;

        await TrySetAsync("set_by_id", OrderByIdKey(order.Id), order, cancellationToken);
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken)
    {
        await TryRemoveAsync("invalidate_all", AllOrdersKey, cancellationToken);
    }

    public async Task InvalidateByIdAsync(long id, CancellationToken cancellationToken)
    {
        await TryRemoveAsync("invalidate_by_id", OrderByIdKey(id), cancellationToken);
    }

    private static string OrderByIdKey(long id) => $"{OrderByIdKeyPrefix}{id}";

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
