using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Add_PurchaseReturnRequest : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "PurchaseReturnRequestId",
            table: "AppPurchaseReturns",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "AppPurchaseReturnRequests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                ReturnType = table.Column<int>(type: "integer", nullable: false),
                RequestDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                SubTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                table.PrimaryKey("PK_AppPurchaseReturnRequests", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequests_AppSuppliers_SupplierId",
                    column: x => x.SupplierId,
                    principalTable: "AppSuppliers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequests_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppPurchaseReturnRequestLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseReturnRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseReturnRequestId1 = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId1 = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId1 = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                BaseQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                OriginalUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                DepreciationRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                ReturnUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                TotalPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                FinalPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppPurchaseReturnRequestLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequestLines_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequestLines_AppBaseUnits_UnitId1",
                    column: x => x.UnitId1,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequestLines_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequestLines_AppProducts_ProductId1",
                    column: x => x.ProductId1,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequestLines_AppPurchaseOrderLines_Purchas~",
                    column: x => x.PurchaseOrderLineId,
                    principalTable: "AppPurchaseOrderLines",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequestLines_AppPurchaseOrders_PurchaseOrd~",
                    column: x => x.PurchaseOrderId,
                    principalTable: "AppPurchaseOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequestLines_AppPurchaseReturnRequests_Pur~",
                    column: x => x.PurchaseReturnRequestId,
                    principalTable: "AppPurchaseReturnRequests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnRequestLines_AppPurchaseReturnRequests_Pu~1",
                    column: x => x.PurchaseReturnRequestId1,
                    principalTable: "AppPurchaseReturnRequests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturns_PurchaseReturnRequestId",
            table: "AppPurchaseReturns",
            column: "PurchaseReturnRequestId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_ProductId",
            table: "AppPurchaseReturnRequestLines",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_ProductId1",
            table: "AppPurchaseReturnRequestLines",
            column: "ProductId1");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_PurchaseOrderId",
            table: "AppPurchaseReturnRequestLines",
            column: "PurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_PurchaseOrderLineId",
            table: "AppPurchaseReturnRequestLines",
            column: "PurchaseOrderLineId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_PurchaseReturnRequestId",
            table: "AppPurchaseReturnRequestLines",
            column: "PurchaseReturnRequestId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_PurchaseReturnRequestId1",
            table: "AppPurchaseReturnRequestLines",
            column: "PurchaseReturnRequestId1");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_UnitId",
            table: "AppPurchaseReturnRequestLines",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequestLines_UnitId1",
            table: "AppPurchaseReturnRequestLines",
            column: "UnitId1");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequests_SupplierId",
            table: "AppPurchaseReturnRequests",
            column: "SupplierId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnRequests_WarehouseId",
            table: "AppPurchaseReturnRequests",
            column: "WarehouseId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppPurchaseReturns_AppPurchaseReturnRequests_PurchaseReturn~",
            table: "AppPurchaseReturns",
            column: "PurchaseReturnRequestId",
            principalTable: "AppPurchaseReturnRequests",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppPurchaseReturns_AppPurchaseReturnRequests_PurchaseReturn~",
            table: "AppPurchaseReturns");

        migrationBuilder.DropTable(
            name: "AppPurchaseReturnRequestLines");

        migrationBuilder.DropTable(
            name: "AppPurchaseReturnRequests");

        migrationBuilder.DropIndex(
            name: "IX_AppPurchaseReturns_PurchaseReturnRequestId",
            table: "AppPurchaseReturns");

        migrationBuilder.DropColumn(
            name: "PurchaseReturnRequestId",
            table: "AppPurchaseReturns");
    }
}
