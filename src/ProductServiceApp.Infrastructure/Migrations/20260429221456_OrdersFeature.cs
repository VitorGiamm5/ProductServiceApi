using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductServiceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrdersFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_order_discount_rule",
                schema: "dbSchemaGoodHamburger",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    has_sandwich = table.Column<bool>(type: "boolean", nullable: false),
                    has_fries = table.Column<bool>(type: "boolean", nullable: false),
                    has_refreshment = table.Column<bool>(type: "boolean", nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_date = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    deleted_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_order_discount_rule", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_orders_audit",
                schema: "dbSchemaGoodHamburger",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_date = table.Column<DateTime>(type: "timestamp", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_date = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    deleted_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_orders_audit", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_order",
                schema: "dbSchemaGoodHamburger",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_orders_audit = table.Column<long>(type: "bigint", nullable: true),
                    subtotal_value = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    total_value = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_date = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    deleted_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_order_tb_orders_audit_id_orders_audit",
                        column: x => x.id_orders_audit,
                        principalSchema: "dbSchemaGoodHamburger",
                        principalTable: "tb_orders_audit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_order_product",
                schema: "dbSchemaGoodHamburger",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_order_product", x => new { x.id, x.product_id });
                    table.ForeignKey(
                        name: "FK_tb_order_product_tb_order_id",
                        column: x => x.id,
                        principalSchema: "dbSchemaGoodHamburger",
                        principalTable: "tb_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_order_product_tb_product_product_id",
                        column: x => x.product_id,
                        principalSchema: "dbSchemaGoodHamburger",
                        principalTable: "tb_product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "dbSchemaGoodHamburger",
                table: "tb_order_discount_rule",
                columns: new[] { "id", "created_by_user_id", "created_date", "deleted_by_user_id", "deleted_date", "discount_percentage", "has_fries", "has_refreshment", "has_sandwich", "is_active", "is_deleted", "updated_by_user_id", "updated_date" },
                values: new object[,]
                {
                    { 100000L, null, null, null, null, 20m, true, true, true, true, false, null, null },
                    { 100001L, null, null, null, null, 15m, false, true, true, true, false, null, null },
                    { 100002L, null, null, null, null, 10m, true, false, true, true, false, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_order_id_orders_audit",
                schema: "dbSchemaGoodHamburger",
                table: "tb_order",
                column: "id_orders_audit");

            migrationBuilder.CreateIndex(
                name: "IX_tb_order_discount_rule_has_sandwich_has_fries_has_refreshme~",
                schema: "dbSchemaGoodHamburger",
                table: "tb_order_discount_rule",
                columns: new[] { "has_sandwich", "has_fries", "has_refreshment" },
                unique: true,
                filter: "is_deleted <> true");

            migrationBuilder.CreateIndex(
                name: "IX_tb_order_product_product_id",
                schema: "dbSchemaGoodHamburger",
                table: "tb_order_product",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_order_discount_rule",
                schema: "dbSchemaGoodHamburger");

            migrationBuilder.DropTable(
                name: "tb_order_product",
                schema: "dbSchemaGoodHamburger");

            migrationBuilder.DropTable(
                name: "tb_order",
                schema: "dbSchemaGoodHamburger");

            migrationBuilder.DropTable(
                name: "tb_orders_audit",
                schema: "dbSchemaGoodHamburger");
        }
    }
}
