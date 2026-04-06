using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class MasSKUBin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxWeight",
                table: "AppBins");

            migrationBuilder.AddColumn<int>(
                name: "MaxSKU",
                table: "AppBins",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSKU",
                table: "AppBins");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxWeight",
                table: "AppBins",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
