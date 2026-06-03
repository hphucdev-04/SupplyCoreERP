using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Add_PurchaseReturn_And_SalesRecall : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppPurchaseReturns",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                ReturnDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                table.PrimaryKey("PK_AppPurchaseReturns", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturns_AppPurchaseOrders_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalTable: "AppPurchaseOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturns_AppSuppliers_SupplierId",
                    column: x => x.SupplierId,
                    principalTable: "AppSuppliers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturns_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppSalesRecalls",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                RecallDecisionNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                RecallDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                Level = table.Column<int>(type: "integer", nullable: false),
                Deadline = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
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
                table.PrimaryKey("PK_AppSalesRecalls", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppSalesRecalls_AppProductBatches_ProductBatchId",
                    column: x => x.ProductBatchId,
                    principalTable: "AppProductBatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_AppSalesRecalls_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesRecalls_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppPurchaseReturnLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                OriginalUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                DepreciationRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppPurchaseReturnLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnLines_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnLines_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseReturnLines_AppPurchaseReturns_PurchaseReturnId",
                    column: x => x.PurchaseReturnId,
                    principalTable: "AppPurchaseReturns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppSalesRecallLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SalesRecallId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                OriginalUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppSalesRecallLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppSalesRecallLines_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesRecallLines_AppCustomers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "AppCustomers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesRecallLines_AppSalesOrders_SalesOrderId",
                    column: x => x.SalesOrderId,
                    principalTable: "AppSalesOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesRecallLines_AppSalesRecalls_SalesRecallId",
                    column: x => x.SalesRecallId,
                    principalTable: "AppSalesRecalls",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnLines_ProductId",
            table: "AppPurchaseReturnLines",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnLines_PurchaseReturnId",
            table: "AppPurchaseReturnLines",
            column: "PurchaseReturnId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturnLines_UnitId",
            table: "AppPurchaseReturnLines",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturns_PurchaseOrderId",
            table: "AppPurchaseReturns",
            column: "PurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturns_SupplierId",
            table: "AppPurchaseReturns",
            column: "SupplierId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseReturns_WarehouseId",
            table: "AppPurchaseReturns",
            column: "WarehouseId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesRecallLines_CustomerId",
            table: "AppSalesRecallLines",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesRecallLines_SalesOrderId",
            table: "AppSalesRecallLines",
            column: "SalesOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesRecallLines_SalesRecallId",
            table: "AppSalesRecallLines",
            column: "SalesRecallId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesRecallLines_UnitId",
            table: "AppSalesRecallLines",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesRecalls_ProductBatchId",
            table: "AppSalesRecalls",
            column: "ProductBatchId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesRecalls_ProductId",
            table: "AppSalesRecalls",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesRecalls_WarehouseId",
            table: "AppSalesRecalls",
            column: "WarehouseId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppPurchaseReturnLines");

        migrationBuilder.DropTable(
            name: "AppSalesRecallLines");

        migrationBuilder.DropTable(
            name: "AppPurchaseReturns");

        migrationBuilder.DropTable(
            name: "AppSalesRecalls");
    }
}
