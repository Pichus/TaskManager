using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectInvite",
                columns: table => new
                {
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    InvitedUserId = table.Column<string>(type: "text", nullable: false),
                    InvitedByUserId = table.Column<string>(type: "text", nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InviteStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInvite", x => new { x.ProjectId, x.InvitedUserId });
                    table.ForeignKey(
                        name: "FK_ProjectInvite_AspNetUsers_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectInvite_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvite_InvitedUserId",
                table: "ProjectInvite",
                column: "InvitedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectInvite");
        }
    }
}
