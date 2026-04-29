using ProductServiceApp.Web.Components;
using ProductServiceApp.Web.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<LoadingState>();
builder.Services.AddHttpClient<ProductApiClient>(client =>
{
    client.BaseAddress = new Uri(GetProductApiBaseAddress(builder.Configuration));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapDefaultEndpoints();

app.Run();

static string GetProductApiBaseAddress(IConfiguration configuration)
{
    var configuredAddress = configuration["ProductApi:BaseAddress"];
    if (!string.IsNullOrWhiteSpace(configuredAddress))
    {
        return configuredAddress;
    }

    return string.Equals(
        configuration["DOTNET_RUNNING_IN_CONTAINER"],
        "true",
        StringComparison.OrdinalIgnoreCase)
            ? "http://6137_api_product_service:9005"
            : "http://localhost:9005";
}
