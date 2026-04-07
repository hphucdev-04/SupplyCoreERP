using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class POSO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "AppPurchaseOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "AppPurchaseOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PriceListId",
                table: "AppCustomers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppInventoryReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceDocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    BinId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservedQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppInventoryReservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSalesOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AppSalesOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSalesOrders_AppCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AppCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppSalesOrderDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DeliveredQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_AppSalesOrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSalesOrderDetails_AppBaseUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "AppBaseUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppSalesOrderDetails_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppSalesOrderDetails_AppSalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "AppSalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_PriceListId",
                table: "AppCustomers",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryReservations_ReferenceDocumentId_Status",
                table: "AppInventoryReservations",
                columns: new[] { "ReferenceDocumentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSalesOrderDetails_ProductId",
                table: "AppSalesOrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalesOrderDetails_SalesOrderId",
                table: "AppSalesOrderDetails",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalesOrderDetails_UnitId",
                table: "AppSalesOrderDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSalesOrders_Code",
                table: "AppSalesOrders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSalesOrders_CustomerId",
                table: "AppSalesOrders",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppCustomers_AppPriceLists_PriceListId",
                table: "AppCustomers",
                column: "PriceListId",
                principalTable: "AppPriceLists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCustomers_AppPriceLists_PriceListId",
                table: "AppCustomers");

            migrationBuilder.DropTable(
                name: "AppInventoryReservations");

            migrationBuilder.DropTable(
                name: "AppSalesOrderDetails");

            migrationBuilder.DropTable(
                name: "AppSalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_AppCustomers_PriceListId",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "AppPurchaseOrders");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "AppPurchaseOrders");

            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "AppCustomers");
        }
    }
}
