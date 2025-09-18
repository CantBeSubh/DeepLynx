using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRbacTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.AlterColumn<long>(
                name: "role_id",
                schema: "deeplynx",
                table: "project_members",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "events",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "events",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "role_id",
                principalSchema: "deeplynx",
                principalTable: "roles",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.AlterColumn<long>(
                name: "role_id",
                schema: "deeplynx",
                table: "project_members",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "events",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "role_id",
                principalSchema: "deeplynx",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
