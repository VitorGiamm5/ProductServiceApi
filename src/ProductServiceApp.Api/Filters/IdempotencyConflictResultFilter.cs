using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductServiceApp.Api.Controllers.Base.BaseApiResponse;

namespace ProductServiceApp.Api.Filters;

public sealed class IdempotencyConflictResultFilter : IResultFilter, IOrderedFilter
{
    private const string PayloadMismatchMessagePrefix = "The Idempotency header key value";

    public int Order => int.MinValue;

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is BadRequestObjectResult { Value: string message } &&
            message.StartsWith(PayloadMismatchMessagePrefix, StringComparison.Ordinal))
        {
            context.Result = new ObjectResult(ApiResponse<object>.SingleFailure(
                409,
                "A chave IdempotencyKey ja foi usada com uma requisicao diferente."))
            {
                StatusCode = StatusCodes.Status409Conflict
            };
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
