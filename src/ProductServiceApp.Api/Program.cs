using Asp.Versioning;
using ProductServiceApp.Api.Conveters;
using ProductServiceApp.Application;
using ProductServiceApp.Application.Metrics;
using ProductServiceApp.Application.Middlewares;
using ProductServiceApp.Infrastructure;
using ProductServiceApp.Infrastructure.Database.Services;
using Prometheus;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Log configuration

// Bootstrap logger — It catches errors BEFORE the host comes up.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Iniciando aplicação...");

#endregion

builder.Configuration.AddEnvironmentVariables();

#region Serilog

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

#endregion

#region Kestrel

builder.WebHost
    .UseUrls() // Clear URLs defaults of Kestrel
    .ConfigureKestrel(options =>
    {
        var kestrelPort = builder.Configuration.GetValue<int>("Kestrel:Port", 5000);

        options.ListenAnyIP(kestrelPort);

        options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(
            builder.Configuration.GetValue<int>("KeepAliveTimeoutSeconds", 30));

        options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(
            builder.Configuration.GetValue<int>("RequestHeadersTimeoutSeconds", 15));
    });

#endregion

#region Controllers

builder.Services.AddControllers(mvc =>
{
    mvc.SuppressAsyncSuffixInActionNames = false;
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.DefaultBufferSize = 4096;

        options.JsonSerializerOptions.Converters.Add(new TrimStringJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter("s"));
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = InvalidModelStateFactory.ExecuteAsync();
    });

#endregion

#region API Versioning

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

#endregion

#region Swagger / OpenAPI

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .WithMethods("GET", "POST", "PUT", "DELETE");
    });
});

#endregion

#region Context Acessor

builder.Services.AddHttpContextAccessor();

#endregion

#region Application DI

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

#endregion

#region App build

var app = builder.Build();

#endregion

#region Logging

app.UseMiddleware<MetricsMiddleware>();
app.UseMetricServer();      // /metrics
app.UseHttpMetrics();       // Collect automatic HTTP measurements (status, duration, size)

// Logs each HTTP request automatically.
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000}ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});

#endregion

#region Migrations

await ExecutePendingMigration.ExecuteAsync(app.Services);

#endregion

#region Middleware Pipeline

if (app.Environment.IsDevelopment())
{
    UseOpenAPI(app);
}

app.UseMiddleware<JsonDeserializationExceptionMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

#endregion

#region Run

await app.RunAsync();

#endregion

#region OpenApi functions

static void UseOpenAPI(WebApplication app)
{
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

#endregion
