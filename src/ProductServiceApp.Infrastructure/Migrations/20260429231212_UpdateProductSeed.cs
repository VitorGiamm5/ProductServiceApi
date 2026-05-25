using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductServiceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "id");

            migrationBuilder.InsertData(
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                columns: new[] { "id", "CreatedByUserId", "CreatedDate", "DeletedByUserId", "DeletedDate", "IsActive", "IsDeleted", "name", "price", "type", "UpdatedByUserId", "UpdatedDate" },
                values: new object[,]
                {
                    { 100000L, 1L, new DateTime(2026, 4, 29, 23, 12, 12, 284, DateTimeKind.Utc).AddTicks(3527), null, null, true, false, "Batata frita", 2m, (byte)2, null, null },
                    { 100001L, 1L, new DateTime(2026, 4, 29, 23, 12, 12, 284, DateTimeKind.Utc).AddTicks(3947), null, null, true, false, "X Burger", 5m, (byte)1, null, null },
                    { 100002L, 1L, new DateTime(2026, 4, 29, 23, 12, 12, 284, DateTimeKind.Utc).AddTicks(4127), null, null, true, false, "X Egg", 4.50m, (byte)1, null, null },
                    { 100003L, 1L, new DateTime(2026, 4, 29, 23, 12, 12, 284, DateTimeKind.Utc).AddTicks(4131), null, null, true, false, "X Bacon", 7m, (byte)1, null, null },
                    { 100004L, 1L, new DateTime(2026, 4, 29, 23, 12, 12, 284, DateTimeKind.Utc).AddTicks(4133), null, null, true, false, "Refrigerante", 2.50m, (byte)3, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                keyColumn: "id",
                keyValue: 100000L);

            migrationBuilder.DeleteData(
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                keyColumn: "id",
                keyValue: 100001L);

            migrationBuilder.DeleteData(
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                keyColumn: "id",
                keyValue: 100002L);

            migrationBuilder.DeleteData(
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                keyColumn: "id",
                keyValue: 100003L);

            migrationBuilder.DeleteData(
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                keyColumn: "id",
                keyValue: 100004L);

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "Id");
        }
    }
}
