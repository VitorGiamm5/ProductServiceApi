using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductServiceApp.Api.Controllers.Base.BaseApiResponse;

public class ApiResponseFilter : IAsyncResultFilter
{
    private readonly IHostEnvironment _env;

    public ApiResponseFilter(IHostEnvironment env) => _env = env;

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            Int16 statusCode = (short)(objectResult.StatusCode ?? 200);

            object finalResponse;

            if (statusCode >= 400)
            {
                var details = _env.IsDevelopment() ? objectResult.Value : null;

                finalResponse = new
                {
                    errors = new List<ApiErrorDetail>
                    {
                        new()
                        {
                            Code = statusCode,
                            Message = GetDefaultMessage(statusCode),
                            Details = details
                        }
                    }
                };
            }
            else
            {
                finalResponse = new
                {
                    data = objectResult.Value ?? new { },
                    errors = Array.Empty<ApiErrorDetail>()
                };
            }

            context.Result = new ObjectResult(finalResponse) { StatusCode = statusCode };
        }

        await next();
    }

    private static string GetDefaultMessage(Int16 statusCode) => statusCode switch
    {
        400 => "Requisição inválida.",
        401 => "Não autorizado.",
        403 => "Acesso negado.",
        404 => "Recurso não encontrado.",
        409 => "Conflito com o estado atual do recurso.",
        422 => "Erro de validação.",
        500 => "Erro interno do servidor.",
        _ => "Erro inesperado."
    };
}
