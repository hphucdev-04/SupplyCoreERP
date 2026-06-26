using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Add_UnitIdName_To_Transaction_Reservation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "UnitId",
            table: "AppInventoryTransactions",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UnitName",
            table: "AppInventoryTransactions",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "UnitId",
            table: "AppInventoryReservations",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UnitName",
            table: "AppInventoryReservations",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTransactions_UnitId",
            table: "AppInventoryTransactions",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryReservations_UnitId",
            table: "AppInventoryReservations",
            column: "UnitId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryReservations_AppBaseUnits_UnitId",
            table: "AppInventoryReservations",
            column: "UnitId",
            principalTable: "AppBaseUnits",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryTransactions_AppBaseUnits_UnitId",
            table: "AppInventoryTransactions",
            column: "UnitId",
            principalTable: "AppBaseUnits",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryReservations_AppBaseUnits_UnitId",
            table: "AppInventoryReservations");

        migrationBuilder.DropForeignKey(
            name: "FK_AppInventoryTransactions_AppBaseUnits_UnitId",
            table: "AppInventoryTransactions");

        migrationBuilder.DropIndex(
            name: "IX_AppInventoryTransactions_UnitId",
            table: "AppInventoryTransactions");

        migrationBuilder.DropIndex(
            name: "IX_AppInventoryReservations_UnitId",
            table: "AppInventoryReservations");

        migrationBuilder.DropColumn(
            name: "UnitId",
            table: "AppInventoryTransactions");

        migrationBuilder.DropColumn(
            name: "UnitName",
            table: "AppInventoryTransactions");

        migrationBuilder.DropColumn(
            name: "UnitId",
            table: "AppInventoryReservations");

        migrationBuilder.DropColumn(
            name: "UnitName",
            table: "AppInventoryReservations");
    }
}
