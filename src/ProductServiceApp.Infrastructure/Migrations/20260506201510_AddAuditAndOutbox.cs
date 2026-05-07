using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProductServiceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_audit_log",
                schema: "dbSchemaGoodHamburger",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity_name = table.Column<string>(type: "varchar", maxLength: 200, nullable: false),
                    entity_key = table.Column<string>(type: "varchar", maxLength: 200, nullable: false),
                    action = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    old_values = table.Column<string>(type: "jsonb", nullable: true),
                    new_values = table.Column<string>(type: "jsonb", nullable: true),
                    user_id = table.Column<string>(type: "varchar", maxLength: 120, nullable: false),
                    user_name = table.Column<string>(type: "varchar", maxLength: 200, nullable: false),
                    correlation_id = table.Column<string>(type: "varchar", maxLength: 120, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_outbox_message",
                schema: "dbSchemaGoodHamburger",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_type = table.Column<string>(type: "varchar", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_outbox_message", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_audit_log_created_at",
                schema: "dbSchemaGoodHamburger",
                table: "tb_audit_log",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_tb_audit_log_entity_name_entity_key",
                schema: "dbSchemaGoodHamburger",
                table: "tb_audit_log",
                columns: new[] { "entity_name", "entity_key" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_outbox_message_status_occurred_at",
                schema: "dbSchemaGoodHamburger",
                table: "tb_outbox_message",
                columns: new[] { "status", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_audit_log",
                schema: "dbSchemaGoodHamburger");

            migrationBuilder.DropTable(
                name: "tb_outbox_message",
                schema: "dbSchemaGoodHamburger");
        }
    }
}
