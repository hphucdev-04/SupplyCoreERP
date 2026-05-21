using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class AddWarehouseToPR : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "WarehouseId",
            table: "AppPurchaseRequisitions",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseRequisitions_WarehouseId",
            table: "AppPurchaseRequisitions",
            column: "WarehouseId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseRequisitions_AppWarehouses_WarehouseId",
            table: "AppPurchaseRequisitions",
            column: "WarehouseId",
            principalTable: "AppWarehouses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseRequisitions_AppWarehouses_WarehouseId",
            table: "AppPurchaseRequisitions");

        migrationBuilder.DropIndex(
            name: "IX_AppPurchaseRequisitions_WarehouseId",
            table: "AppPurchaseRequisitions");

        migrationBuilder.DropColumn(
            name: "WarehouseId",
            table: "AppPurchaseRequisitions");
    }
}
