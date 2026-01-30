using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class Simplify_Manufacturer_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppManufacturers_AppAreas_AreaId",
                table: "AppManufacturers");

            migrationBuilder.DropForeignKey(
                name: "FK_AppManufacturers_AppCities_CityId",
                table: "AppManufacturers");

            migrationBuilder.DropIndex(
                name: "IX_AppManufacturers_AreaId",
                table: "AppManufacturers");

            migrationBuilder.DropIndex(
                name: "IX_AppManufacturers_CityId",
                table: "AppManufacturers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AppManufacturers");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "AppManufacturers");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "AppManufacturers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AppManufacturers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "AppManufacturers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "AppManufacturers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppManufacturers_AreaId",
                table: "AppManufacturers",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppManufacturers_CityId",
                table: "AppManufacturers",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppManufacturers_AppAreas_AreaId",
                table: "AppManufacturers",
                column: "AreaId",
                principalTable: "AppAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppManufacturers_AppCities_CityId",
                table: "AppManufacturers",
                column: "CityId",
                principalTable: "AppCities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
