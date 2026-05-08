using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Add_DocumentSequence_Table : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DocumentSequences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DocumentType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                PrefixDate = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                LastValue = table.Column<int>(type: "integer", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentSequences", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrders_WarehouseId",
            table: "AppSalesOrders",
            column: "WarehouseId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrders_WarehouseId",
            table: "AppPurchaseOrders",
            column: "WarehouseId");

        migrationBuilder.CreateIndex(
            name: "IX_DocumentSequences_DocumentType",
            table: "DocumentSequences",
            column: "DocumentType",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseOrders_AppWarehouses_WarehouseId",
            table: "AppPurchaseOrders",
            column: "WarehouseId",
            principalTable: "AppWarehouses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AppSalesOrders_AppWarehouses_WarehouseId",
            table: "AppSalesOrders",
            column: "WarehouseId",
            principalTable: "AppWarehouses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseOrders_AppWarehouses_WarehouseId",
            table: "AppPurchaseOrders");

        migrationBuilder.DropForeignKey(
            name: "FK_AppSalesOrders_AppWarehouses_WarehouseId",
            table: "AppSalesOrders");

        migrationBuilder.DropTable(
            name: "DocumentSequences");

        migrationBuilder.DropIndex(
            name: "IX_AppSalesOrders_WarehouseId",
            table: "AppSalesOrders");

        migrationBuilder.DropIndex(
            name: "IX_AppPurchaseOrders_WarehouseId",
            table: "AppPurchaseOrders");
    }
}
