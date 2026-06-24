using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Add_Elicitation_To_AgentSession : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ElicitationFormJson",
            table: "AppAgentSessions",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsPendingElicitation",
            table: "AppAgentSessions",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "SuspendedToolCallJson",
            table: "AppAgentSessions",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ElicitationFormJson",
            table: "AppAgentSessions");

        migrationBuilder.DropColumn(
            name: "IsPendingElicitation",
            table: "AppAgentSessions");

        migrationBuilder.DropColumn(
            name: "SuspendedToolCallJson",
            table: "AppAgentSessions");
    }
}
