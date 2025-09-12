using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "events");

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
        }
    }
}
