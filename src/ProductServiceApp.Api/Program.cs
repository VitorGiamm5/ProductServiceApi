using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using ProductServiceApp.Api.Conveters;
using ProductServiceApp.Application;
using ProductServiceApp.Application.Middlewares;
using ProductServiceApp.Infrastructure.Database.Contexts;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.DefaultBufferSize = 4096;

    options.JsonSerializerOptions.Converters.Add(new TrimStringJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter("s"));
});

builder.Services.AddHttpContextAccessor();

// Timeout global para todos os requests
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(3);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(3);
});

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = InvalidModelStateFactory();
    });

// Configuração de versionamento de API
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

EnvironmentVariablesExtensions.AddEnvironmentVariables(builder.Configuration);

builder.Services.AddApplication(builder.Configuration);

// Configuração do OpenAPI/Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração de CORS
builder.Services.AddCors(options =>
{
    // Permite qualquer endereço de origem
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .WithMethods("GET", "POST", "PUT", "DELETE");
    });
});

var app = builder.Build();

// Aplica as migrations automaticamente ao iniciar a aplicação
await using (var scope = app.Services.CreateAsyncScope())
{
    var retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions
        {
            MaxRetryAttempts = 10,
            Delay = TimeSpan.FromSeconds(5),
            BackoffType = DelayBackoffType.Constant,
            OnRetry = args =>
            {
                Console.WriteLine($"[Migration] Tentativa {args.AttemptNumber + 1} — aguardando banco subir... ({args.Outcome.Exception?.Message})");
                return ValueTask.CompletedTask;
            }
        })
        .Build();

    await retryPipeline.ExecuteAsync(async ct =>
    {
        Console.WriteLine("Aplicando migrations...");
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(ct);
        Console.WriteLine("Migrations aplicadas com sucesso.");
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    UseOpenAPI(app);
}

app.UseMiddleware<JsonDeserializationExceptionMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

#region Addictional functions

static void UseOpenAPI(WebApplication app)
{
    // Redireciona a rota raiz para o Swagger
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty; // Abre o Swagger na raiz "/"
    });
}

static Func<ActionContext, IActionResult> InvalidModelStateFactory()
{
    return context =>
    {
        //var logger = context.HttpContext.RequestServices
        //    .GetRequiredService<ILogger<Program>>();

        var isJsonError = context.ModelState
            .Any(e => e.Value?.Errors
                .Any(x => x.ErrorMessage.Contains("JSON") ||
                          x.ErrorMessage.Contains("invalid") ||
                          x.ErrorMessage.Contains("Path: $")) == true);

        if (isJsonError)
        {
            //logger.LogWarning("Malformed JSON on {Method} {Path}",
            //    context.HttpContext.Request.Method,
            //    context.HttpContext.Request.Path);

            var result = new ObjectResult(new
            {
                data = new { },
                errors = new
                {
                    type = "https://tools.ietf.org/html/rfc7807",
                    title = "Invalid JSON payload",
                    status = StatusCodes.Status400BadRequest,
                    detail = "O corpo da requisição contém JSON malformado ou caracteres inválidos.",
                    messages = context.ModelState
                        .Where(e => e.Value?.Errors.Any() == true)
                        .SelectMany(e => e.Value!.Errors.Select(x => x.ErrorMessage))
                        .ToList(),
                    path = context.HttpContext.Request.Path.Value,
                    traceId = context.HttpContext.TraceIdentifier
                }
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };

            result.ContentTypes.Add("application/problem+json");
            return result;
        }

        // Erros de validação normais
        var validationResult = new ObjectResult(new
        {
            data = new { },
            errors = new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Validation failed",
                status = StatusCodes.Status400BadRequest,
                messages = context.ModelState
                    .Where(e => e.Value?.Errors.Any() == true)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                    ),
                path = context.HttpContext.Request.Path.Value,
                traceId = context.HttpContext.TraceIdentifier
            }
        })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        validationResult.ContentTypes.Add("application/problem+json");
        return validationResult;
    };
}

#endregion
