using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductServiceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GrantSchemaPermition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'read_randandan') THEN
                        GRANT USAGE ON SCHEMA "dbSchemaGoodHamburger" TO read_randandan;
                        GRANT SELECT ON ALL TABLES IN SCHEMA "dbSchemaGoodHamburger" TO read_randandan;
                        ALTER DEFAULT PRIVILEGES IN SCHEMA "dbSchemaGoodHamburger" GRANT SELECT ON TABLES TO read_randandan;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'read_randandan') THEN
                        ALTER DEFAULT PRIVILEGES IN SCHEMA "dbSchemaGoodHamburger" REVOKE SELECT ON TABLES FROM read_randandan;
                        REVOKE SELECT ON ALL TABLES IN SCHEMA "dbSchemaGoodHamburger" FROM read_randandan;
                        REVOKE USAGE ON SCHEMA "dbSchemaGoodHamburger" FROM read_randandan;
                    END IF;
                END $$;
                """);
        }
    }
}
