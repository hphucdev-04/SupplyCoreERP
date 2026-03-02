using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class Add_Warehouse_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppProductBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ManufacturingDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppProductBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProductBatches_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProductBatches_AppSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "AppSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppWarehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CityId = table.Column<Guid>(type: "uuid", nullable: true),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    MapWidth = table.Column<int>(type: "integer", nullable: false),
                    MapLength = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AppWarehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWarehouses_AppAreas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "AppAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppWarehouses_AppCities_CityId",
                        column: x => x.CityId,
                        principalTable: "AppCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppInventoryTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AppInventoryTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventoryTickets_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppStorageLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppStorageLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStorageLocations_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppInventoryBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LockedQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_AppInventoryBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventoryBalances_AppProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "AppProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryBalances_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryBalances_AppStorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "AppStorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryBalances_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppInventoryTicketDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AppInventoryTicketDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventoryTicketDetails_AppInventoryTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "AppInventoryTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppInventoryTicketDetails_AppProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "AppProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryTicketDetails_AppStorageLocations_StorageLocati~",
                        column: x => x.StorageLocationId,
                        principalTable: "AppStorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppInventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    QuantityChanged = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BalanceAfterTransaction = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReferenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppInventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventoryTransactions_AppProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "AppProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryTransactions_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryTransactions_AppStorageLocations_StorageLocatio~",
                        column: x => x.StorageLocationId,
                        principalTable: "AppStorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryTransactions_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryBalances_ProductBatchId",
                table: "AppInventoryBalances",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryBalances_ProductId",
                table: "AppInventoryBalances",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryBalances_StorageLocationId",
                table: "AppInventoryBalances",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryBalances_WarehouseId_StorageLocationId_ProductI~",
                table: "AppInventoryBalances",
                columns: new[] { "WarehouseId", "StorageLocationId", "ProductId", "ProductBatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTicketDetails_ProductBatchId",
                table: "AppInventoryTicketDetails",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTicketDetails_ProductId",
                table: "AppInventoryTicketDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTicketDetails_StorageLocationId",
                table: "AppInventoryTicketDetails",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTicketDetails_TicketId",
                table: "AppInventoryTicketDetails",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTickets_ReferenceDocumentId",
                table: "AppInventoryTickets",
                column: "ReferenceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTickets_TicketNumber",
                table: "AppInventoryTickets",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTickets_WarehouseId",
                table: "AppInventoryTickets",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_ProductBatchId",
                table: "AppInventoryTransactions",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_ProductId",
                table: "AppInventoryTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_StorageLocationId",
                table: "AppInventoryTransactions",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_WarehouseId_ProductId_CreationTime",
                table: "AppInventoryTransactions",
                columns: new[] { "WarehouseId", "ProductId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProductBatches_ProductId_Status_ExpiryDate",
                table: "AppProductBatches",
                columns: new[] { "ProductId", "Status", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProductBatches_SupplierId",
                table: "AppProductBatches",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStorageLocations_WarehouseId",
                table: "AppStorageLocations",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouses_AreaId",
                table: "AppWarehouses",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouses_CityId",
                table: "AppWarehouses",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouses_Code",
                table: "AppWarehouses",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppInventoryBalances");

            migrationBuilder.DropTable(
                name: "AppInventoryTicketDetails");

            migrationBuilder.DropTable(
                name: "AppInventoryTransactions");

            migrationBuilder.DropTable(
                name: "AppInventoryTickets");

            migrationBuilder.DropTable(
                name: "AppProductBatches");

            migrationBuilder.DropTable(
                name: "AppStorageLocations");

            migrationBuilder.DropTable(
                name: "AppWarehouses");
        }
    }
}
