namespace ProductServiceApp.Application.Cache.Warmup;

public sealed class CacheWarmupOptions
{
    public bool Enabled { get; init; } = true;
    public bool RunOnStartup { get; init; } = true;
    public bool BlockStartupUntilComplete { get; init; }
    public int StartupDelaySeconds { get; init; } = 3;
    public int TimeoutSeconds { get; init; } = 20;
    public int MaxDegreeOfParallelism { get; init; } = 2;
    public Dictionary<string, CacheWarmupFeatureOptions> Features { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
