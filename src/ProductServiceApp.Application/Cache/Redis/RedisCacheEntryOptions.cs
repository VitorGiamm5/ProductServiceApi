namespace ProductServiceApp.Application.Cache.Redis;

public sealed record RedisCacheEntryOptions(
    TimeSpan? AbsoluteExpirationRelativeToNow,
    TimeSpan? SlidingExpiration);
