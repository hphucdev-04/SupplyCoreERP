using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class UpdatePOAndTicketStructure : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryTicketDetails_AppInventoryTickets_TicketId",
            table: "AppInventoryTicketDetails");

        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
            table: "AppInventoryTicketDetails");

        migrationBuilder.DropTable(
            name: "AppPurchaseOrderDetails");

        migrationBuilder.RenameColumn(
            name: "TicketId",
            table: "AppInventoryTicketDetails",
            newName: "TicketLineId");

        migrationBuilder.RenameIndex(
            name: "IX_AppInventoryTicketDetails_TicketId",
            table: "AppInventoryTicketDetails",
            newName: "IX_AppInventoryTicketDetails_TicketLineId");

        migrationBuilder.CreateTable(
            name: "AppPurchaseOrderLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                ReceivedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                table.PrimaryKey("PK_AppPurchaseOrderLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrderLines_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrderLines_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrderLines_AppPurchaseOrders_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalTable: "AppPurchaseOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppInventoryTicketLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: true),
                Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                table.PrimaryKey("PK_AppInventoryTicketLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppInventoryTicketLines_AppInventoryTickets_TicketId",
                    column: x => x.TicketId,
                    principalTable: "AppInventoryTickets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppInventoryTicketLines_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppInventoryTicketLines_AppPurchaseOrderLines_PurchaseOrder~",
                    column: x => x.PurchaseOrderLineId,
                    principalTable: "AppPurchaseOrderLines",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketLines_ProductId",
            table: "AppInventoryTicketLines",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketLines_PurchaseOrderLineId",
            table: "AppInventoryTicketLines",
            column: "PurchaseOrderLineId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketLines_TicketId",
            table: "AppInventoryTicketLines",
            column: "TicketId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrderLines_ProductId",
            table: "AppPurchaseOrderLines",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrderLines_PurchaseOrderId",
            table: "AppPurchaseOrderLines",
            column: "PurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrderLines_UnitId",
            table: "AppPurchaseOrderLines",
            column: "UnitId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryTicketDetails_AppInventoryTicketLines_TicketLin~",
            table: "AppInventoryTicketDetails",
            column: "TicketLineId",
            principalTable: "AppInventoryTicketLines",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
            table: "AppInventoryTicketDetails",
            column: "ProductId",
            principalTable: "AppProducts",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryTicketDetails_AppInventoryTicketLines_TicketLin~",
            table: "AppInventoryTicketDetails");

        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
            table: "AppInventoryTicketDetails");

        migrationBuilder.DropTable(
            name: "AppInventoryTicketLines");

        migrationBuilder.DropTable(
            name: "AppPurchaseOrderLines");

        migrationBuilder.RenameColumn(
            name: "TicketLineId",
            table: "AppInventoryTicketDetails",
            newName: "TicketId");

        migrationBuilder.RenameIndex(
            name: "IX_AppInventoryTicketDetails_TicketLineId",
            table: "AppInventoryTicketDetails",
            newName: "IX_AppInventoryTicketDetails_TicketId");

        migrationBuilder.CreateTable(
            name: "AppPurchaseOrderDetails",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                ReceivedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppPurchaseOrderDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrderDetails_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrderDetails_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrderDetails_AppPurchaseOrders_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalTable: "AppPurchaseOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrderDetails_ProductId",
            table: "AppPurchaseOrderDetails",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrderDetails_PurchaseOrderId",
            table: "AppPurchaseOrderDetails",
            column: "PurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrderDetails_UnitId",
            table: "AppPurchaseOrderDetails",
            column: "UnitId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryTicketDetails_AppInventoryTickets_TicketId",
            table: "AppInventoryTicketDetails",
            column: "TicketId",
            principalTable: "AppInventoryTickets",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
            table: "AppInventoryTicketDetails",
            column: "ProductId",
            principalTable: "AppProducts",
            principalColumn: "Id");
    }
}
