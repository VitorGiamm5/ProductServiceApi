using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductServiceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductAuditMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "updated_date");

            migrationBuilder.RenameColumn(
                name: "UpdatedByUserId",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "updated_by_user_id");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DeletedDate",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "deleted_date");

            migrationBuilder.RenameColumn(
                name: "DeletedByUserId",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "deleted_by_user_id");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "created_date");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "created_by_user_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "boolean",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_date",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_date",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "updated_by_user_id",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "UpdatedByUserId");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "deleted_date",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "DeletedDate");

            migrationBuilder.RenameColumn(
                name: "deleted_by_user_id",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "DeletedByUserId");

            migrationBuilder.RenameColumn(
                name: "created_date",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "created_by_user_id",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                newName: "CreatedByUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedDate",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "dbSchemaGoodHamburger",
                table: "tb_product",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldNullable: true);
        }
    }
}
