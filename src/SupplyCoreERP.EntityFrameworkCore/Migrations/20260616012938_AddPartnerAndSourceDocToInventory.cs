using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class AddPartnerAndSourceDocToInventory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "ReferenceDocumentNumber",
            table: "AppInventoryTransactions",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "PartnerId",
            table: "AppInventoryTransactions",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PartnerName",
            table: "AppInventoryTransactions",
            type: "character varying(250)",
            maxLength: 250,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "SourceDocumentId",
            table: "AppInventoryTransactions",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SourceDocumentNumber",
            table: "AppInventoryTransactions",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "PartnerId",
            table: "AppInventoryReservations",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PartnerName",
            table: "AppInventoryReservations",
            type: "character varying(250)",
            maxLength: 250,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "SourceDocumentId",
            table: "AppInventoryReservations",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SourceDocumentNumber",
            table: "AppInventoryReservations",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PartnerId",
            table: "AppInventoryTransactions");

        migrationBuilder.DropColumn(
            name: "PartnerName",
            table: "AppInventoryTransactions");

        migrationBuilder.DropColumn(
            name: "SourceDocumentId",
            table: "AppInventoryTransactions");

        migrationBuilder.DropColumn(
            name: "SourceDocumentNumber",
            table: "AppInventoryTransactions");

        migrationBuilder.DropColumn(
            name: "PartnerId",
            table: "AppInventoryReservations");

        migrationBuilder.DropColumn(
            name: "PartnerName",
            table: "AppInventoryReservations");

        migrationBuilder.DropColumn(
            name: "SourceDocumentId",
            table: "AppInventoryReservations");

        migrationBuilder.DropColumn(
            name: "SourceDocumentNumber",
            table: "AppInventoryReservations");

        migrationBuilder.AlterColumn<string>(
            name: "ReferenceDocumentNumber",
            table: "AppInventoryTransactions",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(50)",
            oldMaxLength: 50,
            oldNullable: true);
    }
}
