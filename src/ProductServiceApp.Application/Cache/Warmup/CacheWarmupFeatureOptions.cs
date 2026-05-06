namespace ProductServiceApp.Application.Cache.Warmup;

public sealed class CacheWarmupFeatureOptions
{
    public bool Enabled { get; init; }
    public bool WarmupAll { get; init; } = true;
    public bool WarmupById { get; init; }
    public int MaxItems { get; init; } = 500;
}
