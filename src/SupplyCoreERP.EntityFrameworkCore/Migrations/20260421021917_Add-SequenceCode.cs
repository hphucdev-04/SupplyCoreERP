using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class AddSequenceCode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Code",
            table: "AppProductBatches",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Code",
            table: "AppManufacturers",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_AppProductBatches_Code",
            table: "AppProductBatches",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppManufacturers_Code",
            table: "AppManufacturers",
            column: "Code",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AppProductBatches_Code",
            table: "AppProductBatches");

        migrationBuilder.DropIndex(
            name: "IX_AppManufacturers_Code",
            table: "AppManufacturers");

        migrationBuilder.DropColumn(
            name: "Code",
            table: "AppProductBatches");

        migrationBuilder.DropColumn(
            name: "Code",
            table: "AppManufacturers");
    }
}
