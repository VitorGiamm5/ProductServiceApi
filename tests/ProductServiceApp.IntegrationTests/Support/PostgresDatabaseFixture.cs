using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ProductServiceApp.IntegrationTests.Support;

public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    private Respawner? _respawner;

    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("dbproducts")
        .WithUsername("randandan")
        .WithPassword("randandan_XLR")
        .Build();

    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["dbSchemaGoodHamburger"]
        });

        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
