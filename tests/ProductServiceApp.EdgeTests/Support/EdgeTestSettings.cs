namespace ProductServiceApp.EdgeTests.Support;

public sealed class EdgeTestSettings
{
    public string WebBaseUrl { get; init; } = Environment.GetEnvironmentVariable("EDGE_WEB_BASE_URL")
        ?? "http://localhost:9011";

    public string Username { get; init; } = Environment.GetEnvironmentVariable("EDGE_USERNAME")
        ?? "operator";

    public string Password { get; init; } = Environment.GetEnvironmentVariable("EDGE_PASSWORD")
        ?? "operator123";

    public bool Headless { get; init; } = !string.Equals(
        Environment.GetEnvironmentVariable("EDGE_HEADLESS"),
        "false",
        StringComparison.OrdinalIgnoreCase);
}
