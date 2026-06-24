using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class IncreaseVolumePrecisionTo8DecimalPlaces : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "Volume",
            table: "AppProductUnits",
            type: "numeric(18,8)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,4)");

        migrationBuilder.AlterColumn<decimal>(
            name: "BaseUnitVolume",
            table: "AppProducts",
            type: "numeric(18,8)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,4)");

        migrationBuilder.AlterColumn<decimal>(
            name: "MaxVolume",
            table: "AppBins",
            type: "numeric(18,8)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,4)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "Volume",
            table: "AppProductUnits",
            type: "numeric(18,4)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,8)");

        migrationBuilder.AlterColumn<decimal>(
            name: "BaseUnitVolume",
            table: "AppProducts",
            type: "numeric(18,4)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,8)");

        migrationBuilder.AlterColumn<decimal>(
            name: "MaxVolume",
            table: "AppBins",
            type: "numeric(18,4)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,8)");
    }
}
