using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MemberRoleProjectId",
                table: "ProjectMembers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "MemberRoleUserId",
                table: "ProjectMembers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MemberRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRoles", x => new { x.UserId, x.ProjectId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_MemberRoleUserId_MemberRoleProjectId",
                table: "ProjectMembers",
                columns: new[] { "MemberRoleUserId", "MemberRoleProjectId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_MemberRoles_MemberRoleUserId_MemberRoleProje~",
                table: "ProjectMembers",
                columns: new[] { "MemberRoleUserId", "MemberRoleProjectId" },
                principalTable: "MemberRoles",
                principalColumns: new[] { "UserId", "ProjectId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_MemberRoles_MemberRoleUserId_MemberRoleProje~",
                table: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "MemberRoles");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_MemberRoleUserId_MemberRoleProjectId",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "MemberRoleProjectId",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "MemberRoleUserId",
                table: "ProjectMembers");
        }
    }
}
