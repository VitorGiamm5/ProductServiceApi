namespace ProductServiceApp.Application.Cache.Warmup;

public sealed record CacheWarmupFeatureResult(
    int ItemsLoaded,
    int ItemsByIdWarmed,
    bool WarmedAll,
    bool WarmedById);
