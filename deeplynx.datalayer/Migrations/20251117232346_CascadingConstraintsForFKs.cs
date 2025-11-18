using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class CascadingConstraintsForFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_events_data_sources_data_source_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "FK_events_projects_project_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "FK_object_storages_organizations_organization_id",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.AddForeignKey(
                name: "events_data_source_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "events_organization_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
                name: "object_storage_organization_id_fkey",
                schema: "deeplynx",
                table: "object_storages",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "events_data_source_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "events_organization_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "object_storage_organization_id_fkey",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.AddForeignKey(
                name: "FK_events_data_sources_data_source_id",
                schema: "deeplynx",
                table: "events",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_events_projects_project_id",
                schema: "deeplynx",
                table: "events",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_object_storages_organizations_organization_id",
                schema: "deeplynx",
                table: "object_storages",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");
        }
    }
}
