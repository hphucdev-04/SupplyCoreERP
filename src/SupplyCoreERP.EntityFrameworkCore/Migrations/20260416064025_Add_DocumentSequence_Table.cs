using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Add_DocumentSequence_Table : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DocumentSequences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DocumentType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                PrefixDate = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                LastValue = table.Column<int>(type: "integer", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentSequences", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DocumentSequences_DocumentType",
            table: "DocumentSequences",
            column: "DocumentType",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DocumentSequences");
    }
}
