using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductServiceApp.Api.Controllers.Base.BaseApiResponse;

namespace ProductServiceApp.Api.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireIdempotencyKeyAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
{
    public int Order => int.MinValue;
    public string HeaderName { get; init; } = "IdempotencyKey";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var headers = context.HttpContext.Request.Headers;

        if (!headers.TryGetValue(HeaderName, out var values) ||
            values.Count != 1 ||
            string.IsNullOrWhiteSpace(values[0]))
        {
            context.Result = BuildFailure("Informe o header IdempotencyKey.");
            return;
        }

        if (!Guid.TryParse(values[0], out var idempotencyKey) ||
            idempotencyKey.ToString("D")[14] != '4')
        {
            context.Result = BuildFailure("O header IdempotencyKey deve ser um UUID v4 valido.");
            return;
        }

        await next();
    }

    private static BadRequestObjectResult BuildFailure(string message)
        => new(ApiResponse<object>.SingleFailure(400, message));
}
