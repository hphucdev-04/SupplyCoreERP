using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class AddRequisitionTraceabilityToPO : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseOrders_AppWarehouses_WarehouseId",
            table: "AppPurchaseOrders");

        migrationBuilder.AddColumn<Guid>(
            name: "PurchaseRequisitionId",
            table: "AppPurchaseOrders",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrders_PurchaseRequisitionId",
            table: "AppPurchaseOrders",
            column: "PurchaseRequisitionId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseOrders_AppPurchaseRequisitions_PurchaseRequisiti~",
            table: "AppPurchaseOrders",
            column: "PurchaseRequisitionId",
            principalTable: "AppPurchaseRequisitions",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseOrders_AppWarehouses_WarehouseId",
            table: "AppPurchaseOrders",
            column: "WarehouseId",
            principalTable: "AppWarehouses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseOrders_AppPurchaseRequisitions_PurchaseRequisiti~",
            table: "AppPurchaseOrders");

        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseOrders_AppWarehouses_WarehouseId",
            table: "AppPurchaseOrders");

        migrationBuilder.DropIndex(
            name: "IX_AppPurchaseOrders_PurchaseRequisitionId",
            table: "AppPurchaseOrders");

        migrationBuilder.DropColumn(
            name: "PurchaseRequisitionId",
            table: "AppPurchaseOrders");

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseOrders_AppWarehouses_WarehouseId",
            table: "AppPurchaseOrders",
            column: "WarehouseId",
            principalTable: "AppWarehouses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
