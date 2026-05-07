using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace ProductServiceApp.Api.Auth;

public static class SetupAuth
{
    public static IServiceCollection AddProductServiceAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authOptions = configuration
            .GetSection("Auth")
            .Get<AuthOptions>() ?? new AuthOptions();

        services.Configure<AuthOptions>(configuration.GetSection("Auth"));

        if (!authOptions.Enabled)
        {
            services.AddAuthorization();
            return services;
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authOptions.Authority;
                options.Audience = authOptions.Audience;
                options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role,
                    ValidateAudience = false,
                    ValidIssuer = string.IsNullOrWhiteSpace(authOptions.BrowserAuthority)
                        ? authOptions.Authority
                        : authOptions.BrowserAuthority
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                            AddKeycloakRealmRoles(identity);

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            AddScopeOrRolePolicy(options, AuthPolicies.ProductsRead);
            AddScopeOrRolePolicy(options, AuthPolicies.ProductsWrite);
            AddScopeOrRolePolicy(options, AuthPolicies.OrdersRead);
            AddScopeOrRolePolicy(options, AuthPolicies.OrdersWrite);
            AddScopeOrRolePolicy(options, AuthPolicies.OrdersViewAll);
            AddScopeOrRolePolicy(options, AuthPolicies.OrdersViewOwn);
        });

        return services;
    }

    private static void AddScopeOrRolePolicy(AuthorizationOptions options, string permission)
    {
        options.AddPolicy(permission, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireAssertion(context =>
                HasScope(context.User, permission) ||
                context.User.IsInRole(permission) ||
                context.User.IsInRole("admin"));
        });
    }

    private static bool HasScope(ClaimsPrincipal user, string scope)
    {
        return user
            .FindAll("scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Contains(scope, StringComparer.Ordinal);
    }

    private static void AddKeycloakRealmRoles(ClaimsIdentity identity)
    {
        var realmAccess = identity.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
            return;

        using var document = JsonDocument.Parse(realmAccess);

        if (!document.RootElement.TryGetProperty("roles", out var roles) ||
            roles.ValueKind != JsonValueKind.Array)
            return;

        foreach (var role in roles.EnumerateArray())
        {
            var roleName = role.GetString();
            if (!string.IsNullOrWhiteSpace(roleName))
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
        }
    }
}
