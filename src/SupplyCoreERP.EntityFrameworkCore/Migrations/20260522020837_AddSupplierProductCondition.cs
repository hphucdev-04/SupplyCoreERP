using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class AddSupplierProductCondition : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DefaultConversionFactor",
            table: "AppSupplierProducts");

        migrationBuilder.DropColumn(
            name: "LastPurchasePrice",
            table: "AppSupplierProducts");

        migrationBuilder.DropColumn(
            name: "MinOrderQuantity",
            table: "AppSupplierProducts");

        migrationBuilder.DropColumn(
            name: "OverDeliveryTolerancePct",
            table: "AppSupplierProducts");

        migrationBuilder.DropColumn(
            name: "StandardPrice",
            table: "AppSupplierProducts");

        migrationBuilder.DropColumn(
            name: "UnderDeliveryTolerancePct",
            table: "AppSupplierProducts");

        migrationBuilder.CreateTable(
            name: "AppSupplierProductConditions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SupplierProductId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                StandardPrice = table.Column<decimal>(type: "numeric", nullable: false),
                LastPurchasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                MinOrderQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                OverDeliveryTolerancePct = table.Column<decimal>(type: "numeric", nullable: false),
                UnderDeliveryTolerancePct = table.Column<decimal>(type: "numeric", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppSupplierProductConditions", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppSupplierProductConditions_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSupplierProductConditions_AppSupplierProducts_SupplierPr~",
                    column: x => x.SupplierProductId,
                    principalTable: "AppSupplierProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppSupplierProductConditions_SupplierProductId_UnitId",
            table: "AppSupplierProductConditions",
            columns: new[] { "SupplierProductId", "UnitId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppSupplierProductConditions_UnitId",
            table: "AppSupplierProductConditions",
            column: "UnitId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppSupplierProductConditions");

        migrationBuilder.AddColumn<int>(
            name: "DefaultConversionFactor",
            table: "AppSupplierProducts",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<decimal>(
            name: "LastPurchasePrice",
            table: "AppSupplierProducts",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "MinOrderQuantity",
            table: "AppSupplierProducts",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "OverDeliveryTolerancePct",
            table: "AppSupplierProducts",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "StandardPrice",
            table: "AppSupplierProducts",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "UnderDeliveryTolerancePct",
            table: "AppSupplierProducts",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);
    }
}
