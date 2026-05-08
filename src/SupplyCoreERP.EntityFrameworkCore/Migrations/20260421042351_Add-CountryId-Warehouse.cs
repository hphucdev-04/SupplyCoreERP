using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class AddCountryIdWarehouse : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "CountryId",
            table: "AppWarehouses",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppWarehouses_CountryId",
            table: "AppWarehouses",
            column: "CountryId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppWarehouses_AppCountries_CountryId",
            table: "AppWarehouses",
            column: "CountryId",
            principalTable: "AppCountries",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppWarehouses_AppCountries_CountryId",
            table: "AppWarehouses");

        migrationBuilder.DropIndex(
            name: "IX_AppWarehouses_CountryId",
            table: "AppWarehouses");

        migrationBuilder.DropColumn(
            name: "CountryId",
            table: "AppWarehouses");
    }
}
