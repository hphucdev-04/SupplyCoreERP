using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class MedicineRegistration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RegistrationNumber",
            table: "AppMedicines");

        migrationBuilder.AddColumn<Guid>(
            name: "MedicineRegistrationId",
            table: "AppProductBatches",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "AppMedicineRegistrations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                RegistrationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ValidFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                ValidTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Note = table.Column<string>(type: "text", nullable: true),
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
                table.PrimaryKey("PK_AppMedicineRegistrations", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppMedicineRegistrations_AppMedicines_MedicineId",
                    column: x => x.MedicineId,
                    principalTable: "AppMedicines",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppProductBatches_MedicineRegistrationId",
            table: "AppProductBatches",
            column: "MedicineRegistrationId");

        migrationBuilder.CreateIndex(
            name: "IX_AppMedicineRegistrations_IsActive",
            table: "AppMedicineRegistrations",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_AppMedicineRegistrations_MedicineId_RegistrationNumber",
            table: "AppMedicineRegistrations",
            columns: new[] { "MedicineId", "RegistrationNumber" });

        migrationBuilder.AddForeignKey(
            name: "FK_AppProductBatches_AppMedicineRegistrations_MedicineRegistra~",
            table: "AppProductBatches",
            column: "MedicineRegistrationId",
            principalTable: "AppMedicineRegistrations",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppProductBatches_AppMedicineRegistrations_MedicineRegistra~",
            table: "AppProductBatches");

        migrationBuilder.DropTable(
            name: "AppMedicineRegistrations");

        migrationBuilder.DropIndex(
            name: "IX_AppProductBatches_MedicineRegistrationId",
            table: "AppProductBatches");

        migrationBuilder.DropColumn(
            name: "MedicineRegistrationId",
            table: "AppProductBatches");

        migrationBuilder.AddColumn<string>(
            name: "RegistrationNumber",
            table: "AppMedicines",
            type: "text",
            nullable: false,
            defaultValue: "");
    }
}
