namespace ProductServiceApp.Application.Cache.Redis;

public interface IRedisCacheClient
{
    Task<string?> GetStringAsync(
        string feature,
        string operation,
        string key,
        CancellationToken cancellationToken);

    Task SetStringAsync(
        string feature,
        string operation,
        string key,
        string payload,
        RedisCacheEntryOptions options,
        CancellationToken cancellationToken);

    Task RemoveAsync(
        string feature,
        string operation,
        string key,
        CancellationToken cancellationToken);
}
