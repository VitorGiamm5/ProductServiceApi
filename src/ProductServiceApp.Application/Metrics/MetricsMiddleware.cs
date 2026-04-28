using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace ProductServiceApp.Application.Metrics;

public class MetricsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Request.Path.Value ?? "unknown";
        var method = context.Request.Method;
        var sw = Stopwatch.StartNew();

        try
        {
            await next(context);

            var status = context.Response.StatusCode.ToString();

            AppMetrics.RequestsTotal
                .WithLabels(endpoint, method, status)
                .Inc();

            AppMetrics.RequestDuration
                .WithLabels(endpoint, method)
                .Observe(sw.Elapsed.TotalSeconds);
        }
        catch (Exception)
        {
            AppMetrics.ErrorsTotal
                .WithLabels("unhandled", endpoint)
                .Inc();
            throw;
        }
    }
}
