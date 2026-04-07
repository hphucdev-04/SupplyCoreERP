using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class Added_Supplier_And_Customer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TaxCode = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CityId = table.Column<Guid>(type: "uuid", nullable: true),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    DebtLimit = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentTermDays = table.Column<int>(type: "integer", nullable: false),
                    CurrentDebt = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("PK_AppCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCustomers_AppAreas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "AppAreas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppCustomers_AppCities_CityId",
                        column: x => x.CityId,
                        principalTable: "AppCities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppCustomers_AppCountries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "AppCountries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppSuppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TaxCode = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    RepresentativeName = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    DebtLimit = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentTermDays = table.Column<int>(type: "integer", nullable: false),
                    CurrentDebt = table.Column<decimal>(type: "numeric", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CityId = table.Column<Guid>(type: "uuid", nullable: true),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppSuppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSuppliers_AppAreas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "AppAreas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppSuppliers_AppCities_CityId",
                        column: x => x.CityId,
                        principalTable: "AppCities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppSuppliers_AppCountries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "AppCountries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_AreaId",
                table: "AppCustomers",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_CityId",
                table: "AppCustomers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_Code",
                table: "AppCustomers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_CountryId",
                table: "AppCustomers",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_PhoneNumber",
                table: "AppCustomers",
                column: "PhoneNumber",
                unique: true,
                filter: "\"PhoneNumber\" IS NOT NULL AND \"PhoneNumber\" != ''");

            migrationBuilder.CreateIndex(
                name: "IX_AppSuppliers_AreaId",
                table: "AppSuppliers",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSuppliers_CityId",
                table: "AppSuppliers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSuppliers_Code",
                table: "AppSuppliers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSuppliers_CountryId",
                table: "AppSuppliers",
                column: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCustomers");

            migrationBuilder.DropTable(
                name: "AppSuppliers");
        }
    }
}
