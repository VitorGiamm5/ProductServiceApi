using Prometheus;

namespace ProductServiceApp.Infrastructure.Database.Metrics;

public static class DatabaseMetrics
{
    public static readonly Counter ReadOperationsTotal = Prometheus.Metrics
        .CreateCounter(
            "app_database_read_operations_total",
            "Total number of database read operations",
            labelNames: ["operation", "context"]);

    public static readonly Counter WriteOperationsTotal = Prometheus.Metrics
        .CreateCounter(
            "app_database_write_operations_total",
            "Total number of database write operations",
            labelNames: ["operation", "context"]);

    static DatabaseMetrics()
    {
        ReadOperationsTotal.WithLabels("select", "ApplicationDbContext").Inc(0);
        ReadOperationsTotal.WithLabels("select", "ReadOnlyDbContext").Inc(0);

        foreach (var operation in new[] { "insert", "update", "delete", "merge" })
        {
            WriteOperationsTotal.WithLabels(operation, "ApplicationDbContext").Inc(0);
        }
    }
}
