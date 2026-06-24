using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSoftDeleteFromInventoryBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppInventoryBalances");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppInventoryBalances");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppInventoryBalances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppInventoryBalances",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppInventoryBalances",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppInventoryBalances",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
