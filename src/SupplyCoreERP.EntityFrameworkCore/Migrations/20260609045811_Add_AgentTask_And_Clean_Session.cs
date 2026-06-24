using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Add_AgentTask_And_Clean_Session : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ElicitationFormJson",
            table: "AppAgentSessions");

        migrationBuilder.DropColumn(
            name: "IsPendingApproval",
            table: "AppAgentSessions");

        migrationBuilder.DropColumn(
            name: "IsPendingElicitation",
            table: "AppAgentSessions");

        migrationBuilder.DropColumn(
            name: "PendingToolCallJson",
            table: "AppAgentSessions");

        migrationBuilder.DropColumn(
            name: "SuspendedToolCallJson",
            table: "AppAgentSessions");

        migrationBuilder.CreateTable(
            name: "AppAgentTasks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                TaskType = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                FormJson = table.Column<string>(type: "text", nullable: true),
                SuspendedDataJson = table.Column<string>(type: "text", nullable: true),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppAgentTasks", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppAgentTasks");

        migrationBuilder.AddColumn<string>(
            name: "ElicitationFormJson",
            table: "AppAgentSessions",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsPendingApproval",
            table: "AppAgentSessions",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsPendingElicitation",
            table: "AppAgentSessions",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "PendingToolCallJson",
            table: "AppAgentSessions",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SuspendedToolCallJson",
            table: "AppAgentSessions",
            type: "text",
            nullable: true);
    }
}
