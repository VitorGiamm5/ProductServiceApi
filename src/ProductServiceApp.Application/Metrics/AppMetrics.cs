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
}
