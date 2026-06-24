using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class Drop_OverUnderDeliveryTolerance_From_SupplierProductConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverDeliveryTolerancePct",
                table: "AppSupplierProductConditions");

            migrationBuilder.DropColumn(
                name: "UnderDeliveryTolerancePct",
                table: "AppSupplierProductConditions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OverDeliveryTolerancePct",
                table: "AppSupplierProductConditions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnderDeliveryTolerancePct",
                table: "AppSupplierProductConditions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
