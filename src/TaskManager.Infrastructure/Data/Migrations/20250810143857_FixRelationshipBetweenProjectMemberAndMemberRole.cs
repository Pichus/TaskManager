using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TaskManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipBetweenProjectMemberAndMemberRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_MemberRoles_MemberRoleUserId_MemberRoleProje~",
                table: "ProjectMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectMembers",
                table: "ProjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_MemberRoleUserId_MemberRoleProjectId",
                table: "ProjectMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberRoles",
                table: "MemberRoles");

            migrationBuilder.DropColumn(
                name: "MemberRoleUserId",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MemberRoles");

            migrationBuilder.RenameColumn(
                name: "MemberRoleProjectId",
                table: "ProjectMembers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "MemberRoles",
                newName: "ProjectMemberId");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ProjectMembers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProjectMembers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "MemberRoles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MemberRoles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectMembers",
                table: "ProjectMembers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberRoles",
                table: "MemberRoles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ProjectId_MemberId",
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberRoles_ProjectMemberId",
                table: "MemberRoles",
                column: "ProjectMemberId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberRoles_ProjectMembers_ProjectMemberId",
                table: "MemberRoles",
                column: "ProjectMemberId",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberRoles_ProjectMembers_ProjectMemberId",
                table: "MemberRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectMembers",
                table: "ProjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_ProjectId_MemberId",
                table: "ProjectMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberRoles",
                table: "MemberRoles");

            migrationBuilder.DropIndex(
                name: "IX_MemberRoles_ProjectMemberId",
                table: "MemberRoles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MemberRoles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MemberRoles");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ProjectMembers",
                newName: "MemberRoleProjectId");

            migrationBuilder.RenameColumn(
                name: "ProjectMemberId",
                table: "MemberRoles",
                newName: "ProjectId");

            migrationBuilder.AlterColumn<long>(
                name: "MemberRoleProjectId",
                table: "ProjectMembers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "MemberRoleUserId",
                table: "ProjectMembers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MemberRoles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectMembers",
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "MemberId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberRoles",
                table: "MemberRoles",
                columns: new[] { "UserId", "ProjectId" });

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
    }
}
