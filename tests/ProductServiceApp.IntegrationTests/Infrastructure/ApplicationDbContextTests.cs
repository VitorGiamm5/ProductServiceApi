using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using ProductServiceApp.Infrastructure.Database.Contexts;
using ProductServiceApp.IntegrationTests.Support;

namespace ProductServiceApp.IntegrationTests.Infrastructure;

public class ApplicationDbContextTests(PostgresDatabaseFixture database) : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task Migrations_Should_Create_Product_Table_In_PostgreSql()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(database.ConnectionString)
            .UseLoggerFactory(NullLoggerFactory.Instance)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            select exists (
                select 1
                from information_schema.tables
                where table_schema = 'dbSchemaGoodHamburger'
                  and table_name = 'tb_product'
            );
            """,
            connection);

        var tableExists = (bool)(await command.ExecuteScalarAsync() ?? false);

        tableExists.Should().BeTrue();
    }
}
