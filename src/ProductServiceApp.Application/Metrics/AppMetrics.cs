using Prometheus;

namespace ProductServiceApp.Application.Metrics;

public static class AppMetrics
{
    // Request counter per endpoint
    public static readonly Counter RequestsTotal = Prometheus.Metrics
        .CreateCounter(
            "app_requests_total",
            "Total number of requests received",
            labelNames: ["endpoint", "method", "status"]);

    // Histogram of request durations
    public static readonly Histogram RequestDuration = Prometheus.Metrics
        .CreateHistogram(
            "app_request_duration_seconds",
            "Duração das requisições em segundos",
            labelNames: ["endpoint", "method"],
            new HistogramConfiguration
            {
                Buckets = Histogram.LinearBuckets(start: 0.01, width: 0.05, count: 20)
            });

    // Item gauge in the channel
    public static readonly Gauge ChannelQueueSize = Prometheus.Metrics
        .CreateGauge(
            "app_channel_queue_size",
            "Number of items pending in the channel",
            labelNames: ["channel"]);

    // Error counter
    public static readonly Counter ErrorsTotal = Prometheus.Metrics
        .CreateCounter(
            "app_errors_total",
            "Total number of errors",
            labelNames: ["type", "endpoint"]);

    public static readonly Counter CacheReadTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_read_total",
            "Total Redis cache read operations",
            labelNames: ["feature", "operation"]);

    public static readonly Counter CacheHitTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_hit_total",
            "Total Redis cache hits",
            labelNames: ["feature", "operation"]);

    public static readonly Counter CacheMissTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_miss_total",
            "Total Redis cache misses",
            labelNames: ["feature", "operation"]);

    public static readonly Counter CacheWriteTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_write_total",
            "Total Redis cache write operations",
            labelNames: ["feature", "operation"]);

    public static readonly Counter CacheWriteSkippedTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_write_skipped_total",
            "Total Redis cache writes skipped by application policy",
            labelNames: ["feature", "operation", "reason"]);

    public static readonly Counter CacheReadErrorTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_read_error_total",
            "Total Redis cache read errors",
            labelNames: ["feature", "operation", "reason"]);

    public static readonly Counter CacheWriteErrorTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_write_error_total",
            "Total Redis cache write errors",
            labelNames: ["feature", "operation", "reason"]);

    public static readonly Counter CacheCircuitOpenTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_circuit_open_total",
            "Total Redis cache operations skipped because circuit breaker is open",
            labelNames: ["feature", "operation", "dependency_operation"]);

    public static readonly Histogram CachePayloadBytes = Prometheus.Metrics
        .CreateHistogram(
            "productservice_cache_payload_bytes",
            "Serialized Redis cache payload size in bytes",
            labelNames: ["feature", "operation"],
            new HistogramConfiguration
            {
                Buckets = [1024, 4096, 16_384, 65_536, 262_144, 1_048_576]
            });

    public static readonly Histogram CacheOperationDuration = Prometheus.Metrics
        .CreateHistogram(
            "productservice_cache_operation_duration_seconds",
            "Redis cache operation duration in seconds",
            labelNames: ["feature", "operation", "dependency_operation"],
            new HistogramConfiguration
            {
                Buckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.3, 0.5, 1]
            });

    public static readonly Histogram CacheWarmupDuration = Prometheus.Metrics
        .CreateHistogram(
            "productservice_cache_warmup_duration_seconds",
            "Cache warmup duration in seconds",
            labelNames: ["feature"],
            new HistogramConfiguration
            {
                Buckets = [0.1, 0.5, 1, 3, 5, 10, 20, 60]
            });

    public static readonly Counter CacheWarmupSuccessTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_warmup_success_total",
            "Total cache warmup feature executions completed successfully",
            labelNames: ["feature"]);

    public static readonly Counter CacheWarmupErrorTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_warmup_error_total",
            "Total cache warmup feature executions that failed",
            labelNames: ["feature", "reason"]);

    public static readonly Counter CacheWarmupItemsTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_warmup_items_total",
            "Total items processed by cache warmup",
            labelNames: ["feature", "stage"]);

    public static readonly Counter CacheWarmupSkippedTotal = Prometheus.Metrics
        .CreateCounter(
            "productservice_cache_warmup_skipped_total",
            "Total cache warmup features skipped",
            labelNames: ["feature", "reason"]);
}
