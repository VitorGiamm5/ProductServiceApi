using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductServiceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderProductQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "quantity",
                schema: "dbSchemaGoodHamburger",
                table: "tb_order_product",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quantity",
                schema: "dbSchemaGoodHamburger",
                table: "tb_order_product");
        }
    }
}
