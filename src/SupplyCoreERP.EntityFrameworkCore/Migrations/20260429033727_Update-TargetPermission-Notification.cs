using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTargetPermissionNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetRole",
                table: "AppNotifications");

            migrationBuilder.AddColumn<List<string>>(
                name: "TargetPermissions",
                table: "AppNotifications",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetPermissions",
                table: "AppNotifications");

            migrationBuilder.AddColumn<string>(
                name: "TargetRole",
                table: "AppNotifications",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }
    }
}
