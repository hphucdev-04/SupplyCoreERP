using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class Added_DosageForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppMedicines_DosageForms_DosageFormId",
                table: "AppMedicines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DosageForms",
                table: "DosageForms");

            migrationBuilder.RenameTable(
                name: "DosageForms",
                newName: "AppDosageForms");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppDosageForms",
                table: "AppDosageForms",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AppDosageForms_Code",
                table: "AppDosageForms",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppMedicines_AppDosageForms_DosageFormId",
                table: "AppMedicines",
                column: "DosageFormId",
                principalTable: "AppDosageForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppMedicines_AppDosageForms_DosageFormId",
                table: "AppMedicines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppDosageForms",
                table: "AppDosageForms");

            migrationBuilder.DropIndex(
                name: "IX_AppDosageForms_Code",
                table: "AppDosageForms");

            migrationBuilder.RenameTable(
                name: "AppDosageForms",
                newName: "DosageForms");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DosageForms",
                table: "DosageForms",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppMedicines_DosageForms_DosageFormId",
                table: "AppMedicines",
                column: "DosageFormId",
                principalTable: "DosageForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
