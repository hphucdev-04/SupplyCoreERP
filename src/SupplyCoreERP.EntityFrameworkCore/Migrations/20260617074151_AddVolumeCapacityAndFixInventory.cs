using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class AddVolumeCapacityAndFixInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryBalances_AppBins_BinId",
                table: "AppInventoryBalances");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryBalances_BinId",
                table: "AppInventoryBalances");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryBalances_WarehouseId_BinId_ProductId_ProductBat~",
                table: "AppInventoryBalances");

            migrationBuilder.DropColumn(
                name: "BinId",
                table: "AppInventoryBalances");

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                table: "AppProductUnits",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseUnitVolume",
                table: "AppProducts",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxVolume",
                table: "AppBins",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AppInventoryBinBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryBalanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    BinId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LockedQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppInventoryBinBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventoryBinBalances_AppBins_BinId",
                        column: x => x.BinId,
                        principalTable: "AppBins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryBinBalances_AppInventoryBalances_InventoryBalan~",
                        column: x => x.InventoryBalanceId,
                        principalTable: "AppInventoryBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryBalances_WarehouseId_ProductId_ProductBatchId",
                table: "AppInventoryBalances",
                columns: new[] { "WarehouseId", "ProductId", "ProductBatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryBinBalances_BinId",
                table: "AppInventoryBinBalances",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryBinBalances_InventoryBalanceId_BinId",
                table: "AppInventoryBinBalances",
                columns: new[] { "InventoryBalanceId", "BinId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppInventoryBinBalances");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryBalances_WarehouseId_ProductId_ProductBatchId",
                table: "AppInventoryBalances");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "AppProductUnits");

            migrationBuilder.DropColumn(
                name: "BaseUnitVolume",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "MaxVolume",
                table: "AppBins");

            migrationBuilder.AddColumn<Guid>(
                name: "BinId",
                table: "AppInventoryBalances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryBalances_BinId",
                table: "AppInventoryBalances",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryBalances_WarehouseId_BinId_ProductId_ProductBat~",
                table: "AppInventoryBalances",
                columns: new[] { "WarehouseId", "BinId", "ProductId", "ProductBatchId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryBalances_AppBins_BinId",
                table: "AppInventoryBalances",
                column: "BinId",
                principalTable: "AppBins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
