using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using ProductServiceApp.Domain.Controller.BaseApiResponse;
using System.Net;
using System.Text.Json;

namespace ProductServiceApp.Application.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var details = _env.IsDevelopment()
                ? new { ex.Message, ex.StackTrace }
                : null;

            var response = ApiResponse<object>.SingleFailure(
                code: (int)HttpStatusCode.InternalServerError,
                message: "Ocorreu um erro interno. Tente novamente mais tarde.",
                details: details
            );

            // garante que data nunca seja null
            var finalResponse = new
            {
                data = response.Data ?? new { },
                errors = response.Errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(finalResponse));
        }
    }
}
