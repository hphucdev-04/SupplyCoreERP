using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryAdjustment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTicketLines_AppSalesOrderLines_SalesOrderLineId",
                table: "AppInventoryTicketLines");

            migrationBuilder.AddColumn<Guid>(
                name: "AdjustmentLineId",
                table: "AppInventoryTicketLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppInventoryAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AppInventoryAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventoryAdjustments_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppInventoryAdjustmentLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProcessedQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_AppInventoryAdjustmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventoryAdjustmentLines_AppBaseUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "AppBaseUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryAdjustmentLines_AppInventoryAdjustments_Adjustm~",
                        column: x => x.AdjustmentId,
                        principalTable: "AppInventoryAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppInventoryAdjustmentLines_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTicketLines_AdjustmentLineId",
                table: "AppInventoryTicketLines",
                column: "AdjustmentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryAdjustmentLines_AdjustmentId",
                table: "AppInventoryAdjustmentLines",
                column: "AdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryAdjustmentLines_ProductId",
                table: "AppInventoryAdjustmentLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryAdjustmentLines_UnitId",
                table: "AppInventoryAdjustmentLines",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryAdjustments_AdjustmentNumber",
                table: "AppInventoryAdjustments",
                column: "AdjustmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryAdjustments_WarehouseId",
                table: "AppInventoryAdjustments",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTicketLines_AppInventoryAdjustmentLines_Adjustm~",
                table: "AppInventoryTicketLines",
                column: "AdjustmentLineId",
                principalTable: "AppInventoryAdjustmentLines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTicketLines_AppSalesOrderLines_SalesOrderLineId",
                table: "AppInventoryTicketLines",
                column: "SalesOrderLineId",
                principalTable: "AppSalesOrderLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTicketLines_AppInventoryAdjustmentLines_Adjustm~",
                table: "AppInventoryTicketLines");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTicketLines_AppSalesOrderLines_SalesOrderLineId",
                table: "AppInventoryTicketLines");

            migrationBuilder.DropTable(
                name: "AppInventoryAdjustmentLines");

            migrationBuilder.DropTable(
                name: "AppInventoryAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTicketLines_AdjustmentLineId",
                table: "AppInventoryTicketLines");

            migrationBuilder.DropColumn(
                name: "AdjustmentLineId",
                table: "AppInventoryTicketLines");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTicketLines_AppSalesOrderLines_SalesOrderLineId",
                table: "AppInventoryTicketLines",
                column: "SalesOrderLineId",
                principalTable: "AppSalesOrderLines",
                principalColumn: "Id");
        }
    }
}
