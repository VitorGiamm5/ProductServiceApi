namespace ProductServiceApp.Application.Cache.Warmup;

public interface ICacheWarmupFeature
{
    string FeatureName { get; }

    Task<CacheWarmupFeatureResult> WarmupAsync(
        CacheWarmupFeatureOptions options,
        CancellationToken cancellationToken);
}
