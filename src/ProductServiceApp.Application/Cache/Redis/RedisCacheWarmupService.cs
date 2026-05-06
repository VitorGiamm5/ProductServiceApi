using Microsoft.Extensions.Hosting;

namespace ProductServiceApp.Application.Cache.Redis;

internal sealed class RedisCacheWarmupService(IRedisCacheClient cacheClient) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cacheClient;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
