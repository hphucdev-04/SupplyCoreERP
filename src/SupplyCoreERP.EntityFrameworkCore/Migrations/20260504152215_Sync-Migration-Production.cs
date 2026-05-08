using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class SyncMigrationProduction : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppSupplierProducts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                DefaultUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                DefaultConversionFactor = table.Column<int>(type: "integer", nullable: false),
                StandardPrice = table.Column<decimal>(type: "numeric", nullable: false),
                LastPurchasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                LeadTimeDays = table.Column<int>(type: "integer", nullable: false),
                MinOrderQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                OverDeliveryTolerancePct = table.Column<decimal>(type: "numeric", nullable: false),
                UnderDeliveryTolerancePct = table.Column<decimal>(type: "numeric", nullable: false),
                IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Note = table.Column<string>(type: "text", nullable: true),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppSupplierProducts", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppSupplierProducts_AppBaseUnits_DefaultUnitId",
                    column: x => x.DefaultUnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSupplierProducts_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSupplierProducts_AppSuppliers_SupplierId",
                    column: x => x.SupplierId,
                    principalTable: "AppSuppliers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppSupplierProducts_DefaultUnitId",
            table: "AppSupplierProducts",
            column: "DefaultUnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSupplierProducts_ProductId",
            table: "AppSupplierProducts",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSupplierProducts_SupplierId_ProductId",
            table: "AppSupplierProducts",
            columns: new[] { "SupplierId", "ProductId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppSupplierProducts");
    }
}
