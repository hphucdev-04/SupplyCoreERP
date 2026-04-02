using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class Update_TicketDetail_Add_Units : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
                table: "AppInventoryTicketDetails");

            migrationBuilder.AddColumn<int>(
                name: "ConversionFactor",
                table: "AppInventoryTicketDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                table: "AppInventoryTicketDetails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTicketDetails_UnitId",
                table: "AppInventoryTicketDetails",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTicketDetails_AppBaseUnits_UnitId",
                table: "AppInventoryTicketDetails",
                column: "UnitId",
                principalTable: "AppBaseUnits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
                table: "AppInventoryTicketDetails",
                column: "ProductId",
                principalTable: "AppProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTicketDetails_AppBaseUnits_UnitId",
                table: "AppInventoryTicketDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
                table: "AppInventoryTicketDetails");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTicketDetails_UnitId",
                table: "AppInventoryTicketDetails");

            migrationBuilder.DropColumn(
                name: "ConversionFactor",
                table: "AppInventoryTicketDetails");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "AppInventoryTicketDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
                table: "AppInventoryTicketDetails",
                column: "ProductId",
                principalTable: "AppProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
