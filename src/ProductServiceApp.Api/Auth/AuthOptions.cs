namespace ProductServiceApp.Api.Auth;

public sealed class AuthOptions
{
    public bool Enabled { get; init; } = true;
    public string Authority { get; init; } = "http://localhost:8081/realms/productservice";
    public string? BrowserAuthority { get; init; } = "http://localhost:8081/realms/productservice";
    public string Audience { get; init; } = "productservice-api";
    public bool RequireHttpsMetadata { get; init; }
}
