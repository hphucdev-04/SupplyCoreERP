using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderLineIdToTicketLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderLineId",
                table: "AppInventoryTicketLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTicketLines_SalesOrderLineId",
                table: "AppInventoryTicketLines",
                column: "SalesOrderLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTicketLines_AppSalesOrderLines_SalesOrderLineId",
                table: "AppInventoryTicketLines",
                column: "SalesOrderLineId",
                principalTable: "AppSalesOrderLines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTicketLines_AppSalesOrderLines_SalesOrderLineId",
                table: "AppInventoryTicketLines");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTicketLines_SalesOrderLineId",
                table: "AppInventoryTicketLines");

            migrationBuilder.DropColumn(
                name: "SalesOrderLineId",
                table: "AppInventoryTicketLines");
        }
    }
}
