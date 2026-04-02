using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class AddTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppInventoryTickets_TicketId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_TicketId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "AppInventoryTransactions");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceDocumentNumber",
                table: "AppInventoryTransactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceDocumentNumber",
                table: "AppInventoryTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "TicketId",
                table: "AppInventoryTransactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_TicketId",
                table: "AppInventoryTransactions",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppInventoryTickets_TicketId",
                table: "AppInventoryTransactions",
                column: "TicketId",
                principalTable: "AppInventoryTickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
