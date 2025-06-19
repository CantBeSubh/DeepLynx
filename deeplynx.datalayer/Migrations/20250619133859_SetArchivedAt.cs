using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class SetArchivedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "users",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "tags",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "roles",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "relationships",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "records",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "projects",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "permissions",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "edges",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "data_sources",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "deeplynx",
                table: "classes",
                newName: "archived_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "users",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "tags",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "roles",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "relationships",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "records",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "projects",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "permissions",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "edges",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "data_sources",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "classes",
                newName: "deleted_at");
        }
    }
}
