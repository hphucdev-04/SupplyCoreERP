using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Add_AgentMessage_Table_And_Refactor_Session : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ConversationHistoryJson",
            table: "AppAgentSessions",
            newName: "ExtraProperties");

        migrationBuilder.AddColumn<string>(
            name: "ConcurrencyStamp",
            table: "AppAgentSessions",
            type: "character varying(40)",
            maxLength: 40,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "AppAgentMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Text = table.Column<string>(type: "text", nullable: true),
                ToolCallsJson = table.Column<string>(type: "jsonb", nullable: true),
                ToolResponsesJson = table.Column<string>(type: "jsonb", nullable: true),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppAgentMessages", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppAgentMessages_AppAgentSessions_SessionId",
                    column: x => x.SessionId,
                    principalTable: "AppAgentSessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppAgentTasks_SessionId",
            table: "AppAgentTasks",
            column: "SessionId");

        migrationBuilder.CreateIndex(
            name: "IX_AppAgentMessages_SessionId",
            table: "AppAgentMessages",
            column: "SessionId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppAgentTasks_AppAgentSessions_SessionId",
            table: "AppAgentTasks",
            column: "SessionId",
            principalTable: "AppAgentSessions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppAgentTasks_AppAgentSessions_SessionId",
            table: "AppAgentTasks");

        migrationBuilder.DropTable(
            name: "AppAgentMessages");

        migrationBuilder.DropIndex(
            name: "IX_AppAgentTasks_SessionId",
            table: "AppAgentTasks");

        migrationBuilder.DropColumn(
            name: "ConcurrencyStamp",
            table: "AppAgentSessions");

        migrationBuilder.RenameColumn(
            name: "ExtraProperties",
            table: "AppAgentSessions",
            newName: "ConversationHistoryJson");
    }
}
