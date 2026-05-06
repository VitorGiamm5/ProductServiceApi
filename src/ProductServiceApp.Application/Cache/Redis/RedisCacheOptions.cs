namespace ProductServiceApp.Application.Cache.Redis;

public sealed class RedisCacheOptions
{
    public string ConnectionString { get; init; } = "localhost:6379";
    public string? ReadConnectionString { get; init; }
    public string? WriteConnectionString { get; init; }
    public string InstanceName { get; init; } = "ProductServiceApp:";
    public int MaxCachePayloadBytes { get; init; } = 262_144;
    public int OperationTimeoutMilliseconds { get; init; } = 300;
    public double CircuitBreakerFailureRatio { get; init; } = 0.5;
    public int CircuitBreakerMinimumThroughput { get; init; } = 10;
    public int CircuitBreakerSamplingSeconds { get; init; } = 30;
    public int CircuitBreakerBreakSeconds { get; init; } = 15;
    public int ProductsAbsoluteExpirationMinutes { get; init; } = 10;
    public int ProductsSlidingExpirationMinutes { get; init; } = 2;
    public int OrdersAbsoluteExpirationMinutes { get; init; } = 10;
    public int OrdersSlidingExpirationMinutes { get; init; } = 2;

    public TimeSpan OperationTimeout => TimeSpan.FromMilliseconds(Math.Max(1, OperationTimeoutMilliseconds));
    public TimeSpan CircuitBreakerSamplingWindow => TimeSpan.FromSeconds(Math.Max(1, CircuitBreakerSamplingSeconds));
    public TimeSpan CircuitBreakerBreakDuration => TimeSpan.FromSeconds(Math.Max(1, CircuitBreakerBreakSeconds));
}
