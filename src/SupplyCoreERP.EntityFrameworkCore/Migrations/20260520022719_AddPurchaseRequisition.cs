using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class AddPurchaseRequisition : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppPurchaseRequisitions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                RequestedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                RequiredDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppPurchaseRequisitions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppPurchaseRequisitionLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseRequisitionId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                OrderedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppPurchaseRequisitionLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPurchaseRequisitionLines_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseRequisitionLines_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseRequisitionLines_AppPurchaseRequisitions_Purchas~",
                    column: x => x.PurchaseRequisitionId,
                    principalTable: "AppPurchaseRequisitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryReservations_BinId",
            table: "AppInventoryReservations",
            column: "BinId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryReservations_ProductBatchId",
            table: "AppInventoryReservations",
            column: "ProductBatchId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryReservations_ProductId",
            table: "AppInventoryReservations",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryReservations_WarehouseId",
            table: "AppInventoryReservations",
            column: "WarehouseId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseRequisitionLines_ProductId",
            table: "AppPurchaseRequisitionLines",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseRequisitionLines_PurchaseRequisitionId",
            table: "AppPurchaseRequisitionLines",
            column: "PurchaseRequisitionId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseRequisitionLines_UnitId",
            table: "AppPurchaseRequisitionLines",
            column: "UnitId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryReservations_AppBins_BinId",
            table: "AppInventoryReservations",
            column: "BinId",
            principalTable: "AppBins",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryReservations_AppProductBatches_ProductBatchId",
            table: "AppInventoryReservations",
            column: "ProductBatchId",
            principalTable: "AppProductBatches",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryReservations_AppProducts_ProductId",
            table: "AppInventoryReservations",
            column: "ProductId",
            principalTable: "AppProducts",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryReservations_AppWarehouses_WarehouseId",
            table: "AppInventoryReservations",
            column: "WarehouseId",
            principalTable: "AppWarehouses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryReservations_AppBins_BinId",
            table: "AppInventoryReservations");

        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryReservations_AppProductBatches_ProductBatchId",
            table: "AppInventoryReservations");

        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryReservations_AppProducts_ProductId",
            table: "AppInventoryReservations");

        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryReservations_AppWarehouses_WarehouseId",
            table: "AppInventoryReservations");

        migrationBuilder.DropTable(
            name: "AppPurchaseRequisitionLines");

        migrationBuilder.DropTable(
            name: "AppPurchaseRequisitions");

        migrationBuilder.DropIndex(
            name: "IX_AppInventoryReservations_BinId",
            table: "AppInventoryReservations");

        migrationBuilder.DropIndex(
            name: "IX_AppInventoryReservations_ProductBatchId",
            table: "AppInventoryReservations");

        migrationBuilder.DropIndex(
            name: "IX_AppInventoryReservations_ProductId",
            table: "AppInventoryReservations");

        migrationBuilder.DropIndex(
            name: "IX_AppInventoryReservations_WarehouseId",
            table: "AppInventoryReservations");
    }
}
