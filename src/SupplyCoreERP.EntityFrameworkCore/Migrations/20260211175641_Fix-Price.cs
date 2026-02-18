using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class FixPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppProductPrices_AppProducts_ProductId1",
                table: "AppProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_AppProductPrices_ProductId1",
                table: "AppProductPrices");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "AppProductPrices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductId1",
                table: "AppProductPrices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppProductPrices_ProductId1",
                table: "AppProductPrices",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppProductPrices_AppProducts_ProductId1",
                table: "AppProductPrices",
                column: "ProductId1",
                principalTable: "AppProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
