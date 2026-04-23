using Microsoft.AspNetCore.Http;

namespace ProductServiceApp.Application.Middlewares;

public class TimeoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _timeoutMs;

    public TimeoutMiddleware(RequestDelegate next, int timeoutMs = 3000)
    {
        _next = next;
        _timeoutMs = timeoutMs;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            context.RequestAborted // já cancela se o client desconectar
        );
        cts.CancelAfter(TimeSpan.FromMilliseconds(_timeoutMs));

        // Substitui o token do contexto pelo novo (com timeout)
        context.RequestAborted = cts.Token;

        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            context.Response.StatusCode = 408;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Tempo limite atingido", cancellationToken: CancellationToken.None);
        }
    }
}
