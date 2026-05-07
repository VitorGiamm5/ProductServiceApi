using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using ProductServiceApp.ServiceDefaults;
using ProductServiceApp.Web.Auth;
using ProductServiceApp.Web.Components;
using ProductServiceApp.Web.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetValue("Kestrel:Port", 9010));
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "productservice-web";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"]
            ?? "http://localhost:8081/realms/productservice";
        options.ClientId = builder.Configuration["Auth:ClientId"]
            ?? "productservice-dev-blazor";
        options.ResponseType = "code";
        options.UsePkce = true;
        options.SaveTokens = true;
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Auth:RequireHttpsMetadata", false);
        options.NonceCookie.SameSite = SameSiteMode.Lax;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Auth:BrowserAuthority"]
                ?? builder.Configuration["Auth:Authority"]
        };
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.IssuerAddress = UseBrowserAuthority(
                    context.ProtocolMessage.IssuerAddress,
                    options.Authority,
                    builder.Configuration["Auth:BrowserAuthority"]);

                return Task.CompletedTask;
            },
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                context.ProtocolMessage.IssuerAddress = UseBrowserAuthority(
                    context.ProtocolMessage.IssuerAddress,
                    options.Authority,
                    builder.Configuration["Auth:BrowserAuthority"]);
                context.ProtocolMessage.PostLogoutRedirectUri = BuildBlazorRootUrl(context.HttpContext);

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<LoadingState>();
builder.Services.AddTransient<AuthenticatedApiHandler>();
builder.Services.AddHttpClient("keycloak-token-management");
builder.Services.AddHttpClient<ProductApiClient>(client =>
{
    client.BaseAddress = new Uri(GetProductApiBaseAddress(builder.Configuration));
}).AddHttpMessageHandler<AuthenticatedApiHandler>();
builder.Services.AddHttpClient<OrderApiClient>(client =>
{
    client.BaseAddress = new Uri(GetProductApiBaseAddress(builder.Configuration));
}).AddHttpMessageHandler<AuthenticatedApiHandler>();

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

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/auth/login", (string? returnUrl) => Results.Challenge(
    new AuthenticationProperties
    {
        RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/products" : returnUrl
    },
    [OpenIdConnectDefaults.AuthenticationScheme]));

app.MapGet("/logout", async (
    HttpContext context,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory) =>
{
    await RevokeKeycloakTokensAsync(context, configuration, httpClientFactory, loggerFactory);

    return Results.SignOut(
        new AuthenticationProperties { RedirectUri = "/login" },
        [CookieAuthenticationDefaults.AuthenticationScheme]);
});

app.MapGet("/auth/logout", () => Results.Redirect("/logout"));

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

static string UseBrowserAuthority(string issuerAddress, string authority, string? browserAuthority)
{
    if (string.IsNullOrWhiteSpace(browserAuthority))
        return issuerAddress;

    return issuerAddress.Replace(authority, browserAuthority, StringComparison.OrdinalIgnoreCase);
}

static string BuildBlazorRootUrl(HttpContext context)
{
    return $"{context.Request.Scheme}://{context.Request.Host}/";
}

static async Task RevokeKeycloakTokensAsync(
    HttpContext context,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory)
{
    var logger = loggerFactory.CreateLogger("KeycloakLogout");
    var tokens = new[]
    {
        new { Value = await context.GetTokenAsync("access_token"), Hint = "access_token" },
        new { Value = await context.GetTokenAsync("refresh_token"), Hint = "refresh_token" }
    };

    var authority = configuration["Auth:Authority"] ?? "http://localhost:8081/realms/productservice";
    var revokeEndpoint = $"{authority.TrimEnd('/')}/protocol/openid-connect/revoke";
    var clientId = configuration["Auth:ClientId"] ?? "productservice-dev-blazor";
    var clientSecret = configuration["Auth:ClientSecret"];
    var httpClient = httpClientFactory.CreateClient("keycloak-token-management");

    foreach (var token in tokens)
    {
        if (string.IsNullOrWhiteSpace(token.Value))
            continue;

        using var request = new HttpRequestMessage(HttpMethod.Post, revokeEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["token"] = token.Value,
                ["token_type_hint"] = token.Hint
            })
        };

        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        try
        {
            using var response = await httpClient.SendAsync(request, context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Keycloak token revocation returned {StatusCode} for {TokenTypeHint}.",
                    response.StatusCode,
                    token.Hint);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Keycloak token revocation failed for {TokenTypeHint}.", token.Hint);
        }
    }
}
