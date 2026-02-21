using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AppCustomers");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "AppSuppliers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "AppCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeName",
                table: "AppCustomers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AppSuppliers");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "RepresentativeName",
                table: "AppCustomers");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AppCustomers",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}
