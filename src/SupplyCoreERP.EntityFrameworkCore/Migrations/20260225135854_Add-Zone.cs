using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class AddZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryBalances_AppStorageLocations_StorageLocationId",
                table: "AppInventoryBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTicketDetails_AppStorageLocations_StorageLocati~",
                table: "AppInventoryTicketDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppStorageLocations_StorageLocatio~",
                table: "AppInventoryTransactions");

            migrationBuilder.DropTable(
                name: "AppStorageLocations");

            migrationBuilder.RenameColumn(
                name: "StorageLocationId",
                table: "AppInventoryTransactions",
                newName: "BinId");

            migrationBuilder.RenameIndex(
                name: "IX_AppInventoryTransactions_StorageLocationId",
                table: "AppInventoryTransactions",
                newName: "IX_AppInventoryTransactions_BinId");

            migrationBuilder.RenameColumn(
                name: "StorageLocationId",
                table: "AppInventoryTicketDetails",
                newName: "BinId");

            migrationBuilder.RenameIndex(
                name: "IX_AppInventoryTicketDetails_StorageLocationId",
                table: "AppInventoryTicketDetails",
                newName: "IX_AppInventoryTicketDetails_BinId");

            migrationBuilder.RenameColumn(
                name: "StorageLocationId",
                table: "AppInventoryBalances",
                newName: "BinId");

            migrationBuilder.RenameIndex(
                name: "IX_AppInventoryBalances_WarehouseId_StorageLocationId_ProductI~",
                table: "AppInventoryBalances",
                newName: "IX_AppInventoryBalances_WarehouseId_BinId_ProductId_ProductBat~");

            migrationBuilder.RenameIndex(
                name: "IX_AppInventoryBalances_StorageLocationId",
                table: "AppInventoryBalances",
                newName: "IX_AppInventoryBalances_BinId");

            migrationBuilder.CreateTable(
                name: "AppZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    StorageCondition = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    Rotation = table.Column<float>(type: "real", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
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
                    table.PrimaryKey("PK_AppZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppZones_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppBins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    Rotation = table.Column<float>(type: "real", nullable: false),
                    MaxWeight = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
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
                    table.PrimaryKey("PK_AppBins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppBins_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppBins_AppZones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "AppZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppBins_WarehouseId_Code",
                table: "AppBins",
                columns: new[] { "WarehouseId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppBins_ZoneId",
                table: "AppBins",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AppZones_WarehouseId_Code",
                table: "AppZones",
                columns: new[] { "WarehouseId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryBalances_AppBins_BinId",
                table: "AppInventoryBalances",
                column: "BinId",
                principalTable: "AppBins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTicketDetails_AppBins_BinId",
                table: "AppInventoryTicketDetails",
                column: "BinId",
                principalTable: "AppBins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppBins_BinId",
                table: "AppInventoryTransactions",
                column: "BinId",
                principalTable: "AppBins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryBalances_AppBins_BinId",
                table: "AppInventoryBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTicketDetails_AppBins_BinId",
                table: "AppInventoryTicketDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppBins_BinId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropTable(
                name: "AppBins");

            migrationBuilder.DropTable(
                name: "AppZones");

            migrationBuilder.RenameColumn(
                name: "BinId",
                table: "AppInventoryTransactions",
                newName: "StorageLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_AppInventoryTransactions_BinId",
                table: "AppInventoryTransactions",
                newName: "IX_AppInventoryTransactions_StorageLocationId");

            migrationBuilder.RenameColumn(
                name: "BinId",
                table: "AppInventoryTicketDetails",
                newName: "StorageLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_AppInventoryTicketDetails_BinId",
                table: "AppInventoryTicketDetails",
                newName: "IX_AppInventoryTicketDetails_StorageLocationId");

            migrationBuilder.RenameColumn(
                name: "BinId",
                table: "AppInventoryBalances",
                newName: "StorageLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_AppInventoryBalances_WarehouseId_BinId_ProductId_ProductBat~",
                table: "AppInventoryBalances",
                newName: "IX_AppInventoryBalances_WarehouseId_StorageLocationId_ProductI~");

            migrationBuilder.RenameIndex(
                name: "IX_AppInventoryBalances_BinId",
                table: "AppInventoryBalances",
                newName: "IX_AppInventoryBalances_StorageLocationId");

            migrationBuilder.CreateTable(
                name: "AppStorageLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    MaxWeight = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStorageLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStorageLocations_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppStorageLocations_WarehouseId",
                table: "AppStorageLocations",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryBalances_AppStorageLocations_StorageLocationId",
                table: "AppInventoryBalances",
                column: "StorageLocationId",
                principalTable: "AppStorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTicketDetails_AppStorageLocations_StorageLocati~",
                table: "AppInventoryTicketDetails",
                column: "StorageLocationId",
                principalTable: "AppStorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppStorageLocations_StorageLocatio~",
                table: "AppInventoryTransactions",
                column: "StorageLocationId",
                principalTable: "AppStorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
