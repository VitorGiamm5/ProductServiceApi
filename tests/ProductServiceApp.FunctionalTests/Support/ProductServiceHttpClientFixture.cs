using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace ProductServiceApp.FunctionalTests.Support;

public sealed class ProductServiceHttpClientFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var externalBaseUrl = Environment.GetEnvironmentVariable("PRODUCT_SERVICE_BASE_URL");

        if (!string.IsNullOrWhiteSpace(externalBaseUrl))
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(externalBaseUrl, UriKind.Absolute)
            };

            return;
        }

        _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("dbproducts")
            .WithUsername("randandan")
            .WithPassword("randandan_XLR")
            .Build();

        await _container.StartAsync();

        _factory = new ProductServiceWebApplicationFactory(_container.GetConnectionString());
        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    private sealed class ProductServiceWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PostgresWrite"] = connectionString,
                    ["ConnectionStrings:PostgresRead"] = connectionString,
                    ["Redis:ConnectionString"] = "localhost:6379",
                    ["Kestrel:Port"] = "0"
                });
            });
        }
    }
}
