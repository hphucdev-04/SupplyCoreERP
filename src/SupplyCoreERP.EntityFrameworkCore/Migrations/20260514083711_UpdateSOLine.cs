using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class UpdateSOLine : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppSalesOrderDetails");

        migrationBuilder.CreateTable(
            name: "AppSalesOrderLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                DeliveredQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                DiscountRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
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
                table.PrimaryKey("PK_AppSalesOrderLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppSalesOrderLines_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesOrderLines_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesOrderLines_AppSalesOrders_SalesOrderId",
                    column: x => x.SalesOrderId,
                    principalTable: "AppSalesOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketLines_UnitId",
            table: "AppInventoryTicketLines",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrderLines_ProductId",
            table: "AppSalesOrderLines",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrderLines_SalesOrderId",
            table: "AppSalesOrderLines",
            column: "SalesOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrderLines_UnitId",
            table: "AppSalesOrderLines",
            column: "UnitId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryTicketLines_AppBaseUnits_UnitId",
            table: "AppInventoryTicketLines",
            column: "UnitId",
            principalTable: "AppBaseUnits",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryTicketLines_AppBaseUnits_UnitId",
            table: "AppInventoryTicketLines");

        migrationBuilder.DropTable(
            name: "AppSalesOrderLines");

        migrationBuilder.DropIndex(
            name: "IX_AppInventoryTicketLines_UnitId",
            table: "AppInventoryTicketLines");

        migrationBuilder.CreateTable(
            name: "AppSalesOrderDetails",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                DeliveredQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                DiscountRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppSalesOrderDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppSalesOrderDetails_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesOrderDetails_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesOrderDetails_AppSalesOrders_SalesOrderId",
                    column: x => x.SalesOrderId,
                    principalTable: "AppSalesOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrderDetails_ProductId",
            table: "AppSalesOrderDetails",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrderDetails_SalesOrderId",
            table: "AppSalesOrderDetails",
            column: "SalesOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrderDetails_UnitId",
            table: "AppSalesOrderDetails",
            column: "UnitId");
    }
}
