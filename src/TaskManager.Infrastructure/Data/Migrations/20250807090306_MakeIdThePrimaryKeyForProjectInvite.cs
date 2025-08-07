using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TaskManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeIdThePrimaryKeyForProjectInvite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectInvites",
                table: "ProjectInvites");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ProjectInvites",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProjectInvites_ProjectId_InvitedUserId",
                table: "ProjectInvites",
                columns: new[] { "ProjectId", "InvitedUserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectInvites",
                table: "ProjectInvites",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProjectInvites_ProjectId_InvitedUserId",
                table: "ProjectInvites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectInvites",
                table: "ProjectInvites");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ProjectInvites",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectInvites",
                table: "ProjectInvites",
                columns: new[] { "ProjectId", "InvitedUserId" });
        }
    }
}
