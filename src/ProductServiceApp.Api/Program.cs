using Asp.Versioning;
using ProductServiceApp.Api.Conveters;
using ProductServiceApp.Application;
using ProductServiceApp.Application.Middlewares;
using ProductServiceApp.Infrastructure.Database.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------ //
// Reading environment variables and replacing them in appsettings
// ------------------------------------------------------------------ //
builder.Configuration.AddEnvironmentVariables();

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

builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

#region Migrations

await ExecutePendingMigration.ExecuteAsync(builder.Services);

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

await app.RunAsync();

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
