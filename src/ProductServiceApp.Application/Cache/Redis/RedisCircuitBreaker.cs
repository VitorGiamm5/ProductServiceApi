using Microsoft.Extensions.Logging;

namespace ProductServiceApp.Application.Cache.Redis;

internal sealed class RedisCircuitBreaker(
    RedisCacheOptions options,
    ILogger logger,
    string dependency)
{
    private readonly object _sync = new();
    private readonly Queue<Sample> _samples = new();
    private DateTimeOffset? _openedUntil;
    private bool _halfOpenProbeInFlight;

    public bool TryEnter()
    {
        lock (_sync)
        {
            var now = DateTimeOffset.UtcNow;

            if (_openedUntil is null)
                return true;

            if (now < _openedUntil.Value)
                return false;

            if (_halfOpenProbeInFlight)
                return false;

            _halfOpenProbeInFlight = true;
            return true;
        }
    }

    public void ReportSuccess()
    {
        lock (_sync)
        {
            _halfOpenProbeInFlight = false;
            _openedUntil = null;
            AddSample(true);
        }
    }

    public void ReportFailure(Exception exception)
    {
        lock (_sync)
        {
            _halfOpenProbeInFlight = false;
            AddSample(false);

            if (ShouldOpen())
                Open(exception);
        }
    }

    private void AddSample(bool success)
    {
        var now = DateTimeOffset.UtcNow;
        _samples.Enqueue(new Sample(now, success));

        while (_samples.Count > 0 && now - _samples.Peek().Timestamp > options.CircuitBreakerSamplingWindow)
            _samples.Dequeue();
    }

    private bool ShouldOpen()
    {
        if (_samples.Count < Math.Max(1, options.CircuitBreakerMinimumThroughput))
            return false;

        var failures = _samples.Count(sample => !sample.Success);
        var ratio = failures / (double)_samples.Count;

        return ratio >= options.CircuitBreakerFailureRatio;
    }

    private void Open(Exception exception)
    {
        _openedUntil = DateTimeOffset.UtcNow.Add(options.CircuitBreakerBreakDuration);
        _samples.Clear();

        logger.LogWarning(
            exception,
            "Redis circuit breaker opened for {RedisDependency} until {OpenedUntil}.",
            dependency,
            _openedUntil);
    }

    private sealed record Sample(DateTimeOffset Timestamp, bool Success);
}
