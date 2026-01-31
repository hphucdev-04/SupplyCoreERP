using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class Add_OriginCountry_To_Medicine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginCountryId",
                table: "AppMedicines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppMedicines_OriginCountryId",
                table: "AppMedicines",
                column: "OriginCountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppMedicines_AppCountries_OriginCountryId",
                table: "AppMedicines",
                column: "OriginCountryId",
                principalTable: "AppCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppMedicines_AppCountries_OriginCountryId",
                table: "AppMedicines");

            migrationBuilder.DropIndex(
                name: "IX_AppMedicines_OriginCountryId",
                table: "AppMedicines");

            migrationBuilder.DropColumn(
                name: "OriginCountryId",
                table: "AppMedicines");
        }
    }
}
