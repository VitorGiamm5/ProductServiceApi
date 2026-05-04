using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductServiceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTypeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_product_type",
                schema: "dbSchemaGoodHamburger",
                columns: table => new
                {
                    id = table.Column<byte>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "varchar", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_product_type", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "dbSchemaGoodHamburger",
                table: "tb_product_type",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { (byte)0, "Nao definido" },
                    { (byte)1, "Sanduiche" },
                    { (byte)2, "Batata frita" },
                    { (byte)3, "Refrigerante" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_product_type",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                column: "type");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_product_tb_product_type_type",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                column: "type",
                principalSchema: "dbSchemaGoodHamburger",
                principalTable: "tb_product_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_product_tb_product_type_type",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product");

            migrationBuilder.DropTable(
                name: "tb_product_type",
                schema: "dbSchemaGoodHamburger");

            migrationBuilder.DropIndex(
                name: "IX_tb_product_type",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product");
        }
    }
}
