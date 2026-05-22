using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class UpgradeToTieredPricing : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AppSupplierProductConditions_SupplierProductId_UnitId",
            table: "AppSupplierProductConditions");

        migrationBuilder.CreateIndex(
            name: "IX_AppSupplierProductConditions_SupplierProductId_UnitId_MinOr~",
            table: "AppSupplierProductConditions",
            columns: new[] { "SupplierProductId", "UnitId", "MinOrderQuantity" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AppSupplierProductConditions_SupplierProductId_UnitId_MinOr~",
            table: "AppSupplierProductConditions");

        migrationBuilder.CreateIndex(
            name: "IX_AppSupplierProductConditions_SupplierProductId_UnitId",
            table: "AppSupplierProductConditions",
            columns: new[] { "SupplierProductId", "UnitId" },
            unique: true);
    }
}
