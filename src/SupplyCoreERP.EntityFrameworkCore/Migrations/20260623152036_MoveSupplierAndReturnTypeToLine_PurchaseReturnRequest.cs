using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class MoveSupplierAndReturnTypeToLine_PurchaseReturnRequest : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseReturnRequests_AppSuppliers_SupplierId",
            table: "AppPurchaseReturnRequests");

        migrationBuilder.DropIndex(
            name: "IX_AppPurchaseReturnRequests_SupplierId",
            table: "AppPurchaseReturnRequests");

        migrationBuilder.DropColumn(
            name: "ReturnType",
            table: "AppPurchaseReturnRequests");

        migrationBuilder.DropColumn(
            name: "SupplierId",
            table: "AppPurchaseReturnRequests");

        migrationBuilder.AddColumn<int>(
            name: "ReturnType",
            table: "AppPurchaseReturnRequestLines",
            type: "integer",
            nullable: false,
            defaultValue: 2);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReturnType",
            table: "AppPurchaseReturnRequestLines");

        migrationBuilder.AddColumn<int>(
            name: "ReturnType",
            table: "AppPurchaseReturnRequests",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<Guid>(
            name: "SupplierId",
            table: "AppPurchaseReturnRequests",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequests_SupplierId",
            table: "AppPurchaseReturnRequests",
            column: "SupplierId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseReturnRequests_AppSuppliers_SupplierId",
            table: "AppPurchaseReturnRequests",
            column: "SupplierId",
            principalTable: "AppSuppliers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
