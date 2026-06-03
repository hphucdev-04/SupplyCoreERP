using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Fix_PurchaseReturnRequest : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseReturnRequestLines_AppBaseUnits_UnitId1",
            table: "AppPurchaseReturnRequestLines");

        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseReturnRequestLines_AppProducts_ProductId1",
            table: "AppPurchaseReturnRequestLines");

        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseReturnRequestLines_AppPurchaseReturnRequests_Pu~1",
            table: "AppPurchaseReturnRequestLines");

        migrationBuilder.DropIndex(
            name: "IX_AppPurchaseReturnRequestLines_ProductId1",
            table: "AppPurchaseReturnRequestLines");

        migrationBuilder.DropIndex(
            name: "IX_AppPurchaseReturnRequestLines_PurchaseReturnRequestId1",
            table: "AppPurchaseReturnRequestLines");

        migrationBuilder.DropIndex(
            name: "IX_AppPurchaseReturnRequestLines_UnitId1",
            table: "AppPurchaseReturnRequestLines");

        migrationBuilder.DropColumn(
            name: "ProductId1",
            table: "AppPurchaseReturnRequestLines");

        migrationBuilder.DropColumn(
            name: "PurchaseReturnRequestId1",
            table: "AppPurchaseReturnRequestLines");

        migrationBuilder.DropColumn(
            name: "UnitId1",
            table: "AppPurchaseReturnRequestLines");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ProductId1",
            table: "AppPurchaseReturnRequestLines",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "PurchaseReturnRequestId1",
            table: "AppPurchaseReturnRequestLines",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "UnitId1",
            table: "AppPurchaseReturnRequestLines",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_ProductId1",
            table: "AppPurchaseReturnRequestLines",
            column: "ProductId1");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_PurchaseReturnRequestId1",
            table: "AppPurchaseReturnRequestLines",
            column: "PurchaseReturnRequestId1");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_UnitId1",
            table: "AppPurchaseReturnRequestLines",
            column: "UnitId1");

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseReturnRequestLines_AppBaseUnits_UnitId1",
            table: "AppPurchaseReturnRequestLines",
            column: "UnitId1",
            principalTable: "AppBaseUnits",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseReturnRequestLines_AppProducts_ProductId1",
            table: "AppPurchaseReturnRequestLines",
            column: "ProductId1",
            principalTable: "AppProducts",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseReturnRequestLines_AppPurchaseReturnRequests_Pu~1",
            table: "AppPurchaseReturnRequestLines",
            column: "PurchaseReturnRequestId1",
            principalTable: "AppPurchaseReturnRequests",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
