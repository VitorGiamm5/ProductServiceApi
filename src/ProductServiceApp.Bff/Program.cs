using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

#region Builder

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetValue("Kestrel:Port", 9020));
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-productservice-bff";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"]
            ?? "http://localhost:8081/realms/productservice";
        options.ClientId = builder.Configuration["Auth:ClientId"]
            ?? "productservice-bff";
        options.ClientSecret = builder.Configuration["Auth:ClientSecret"]
            ?? "productservice-bff-secret";
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Auth:RequireHttpsMetadata", false);
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

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient("product-api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ProductApi:BaseAddress"]
        ?? "http://localhost:9005");
});

#endregion

#region App builder

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Endpoints

app.MapGet("/login", () => Results.Challenge(
    authenticationSchemes: [OpenIdConnectDefaults.AuthenticationScheme]));

app.MapPost("/logout", () => Results.SignOut(
    authenticationSchemes: [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

app.MapGet("/me", (HttpContext context) =>
{
    return Results.Ok(new
    {
        IsAuthenticated = context.User.Identity?.IsAuthenticated == true,
        Name = context.User.Identity?.Name,
        Claims = context.User.Claims.Select(claim => new { claim.Type, claim.Value })
    });
}).RequireAuthorization();

app.MapMethods("/api/{**path}", ["GET", "POST", "PUT", "DELETE"], ProxyApiAsync)
    .RequireAuthorization();

#endregion

await app.RunAsync();

static string UseBrowserAuthority(string issuerAddress, string authority, string? browserAuthority)
{
    if (string.IsNullOrWhiteSpace(browserAuthority))
        return issuerAddress;

    return issuerAddress.Replace(authority, browserAuthority, StringComparison.OrdinalIgnoreCase);
}

static async Task<IResult> ProxyApiAsync(
    string path,
    HttpContext context,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken)
{
    var accessToken = await context.GetTokenAsync("access_token");
    if (string.IsNullOrWhiteSpace(accessToken))
        return Results.Unauthorized();

    var client = httpClientFactory.CreateClient("product-api");
    var targetPath = $"/api/{path}{context.Request.QueryString}";

    using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetPath);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    if (context.Request.ContentLength > 0)
    {
        request.Content = new StreamContent(context.Request.Body);

        if (!string.IsNullOrWhiteSpace(context.Request.ContentType))
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
    }

    var response = await client.SendAsync(
        request,
        HttpCompletionOption.ResponseHeadersRead,
        cancellationToken);

    return new ApiProxyResult(response);
}

internal sealed class ApiProxyResult(HttpResponseMessage responseMessage) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        using var _ = responseMessage;

        httpContext.Response.StatusCode = (int)responseMessage.StatusCode;

        foreach (var header in responseMessage.Headers)
            httpContext.Response.Headers[header.Key] = header.Value.ToArray();

        foreach (var header in responseMessage.Content.Headers)
            httpContext.Response.Headers[header.Key] = header.Value.ToArray();

        httpContext.Response.Headers.Remove("transfer-encoding");

        await responseMessage.Content.CopyToAsync(httpContext.Response.Body);
    }
}
