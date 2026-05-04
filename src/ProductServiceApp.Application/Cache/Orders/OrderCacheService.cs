using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductServiceApp.Domain.Entities.Orders;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductServiceApp.Application.Cache.Orders;

public class OrderCacheService(
    IDistributedCache cache,
    IConfiguration configuration,
    ILogger<OrderCacheService> logger)
    : IOrderCacheService
{
    private const string AllOrdersKey = "orders:all";
    private const string OrderByIdKeyPrefix = "orders:id:";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Redis:OrdersAbsoluteExpirationMinutes", 10)),
        SlidingExpiration = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Redis:OrdersSlidingExpirationMinutes", 2))
    };

    public async Task<OrderEntity[]?> GetAllAsync(CancellationToken cancellationToken)
    {
        return await TryGetAsync<OrderEntity[]>(AllOrdersKey, cancellationToken);
    }

    public async Task SetAllAsync(IEnumerable<OrderEntity> orders, CancellationToken cancellationToken)
    {
        await TrySetAsync(AllOrdersKey, orders.ToArray(), cancellationToken);
    }

    public async Task<OrderEntity?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await TryGetAsync<OrderEntity>(OrderByIdKey(id), cancellationToken);
    }

    public async Task SetByIdAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        if (order.Id <= 0)
            return;

        await TrySetAsync(OrderByIdKey(order.Id), order, cancellationToken);
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken)
    {
        await TryRemoveAsync(AllOrdersKey, cancellationToken);
    }

    public async Task InvalidateByIdAsync(long id, CancellationToken cancellationToken)
    {
        await TryRemoveAsync(OrderByIdKey(id), cancellationToken);
    }

    private static string OrderByIdKey(long id) => $"{OrderByIdKeyPrefix}{id}";

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
