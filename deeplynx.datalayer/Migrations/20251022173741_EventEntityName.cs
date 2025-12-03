using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class EventEntityName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "entity_name",
                schema: "deeplynx",
                table: "events",
                type: "text",
                nullable: true);

            // Clean up orphaned records first
            migrationBuilder.Sql(@"
            DELETE FROM deeplynx.events 
            WHERE data_source_id IS NOT NULL 
            AND NOT EXISTS (
                SELECT 1 FROM deeplynx.data_sources 
                WHERE id = events.data_source_id
            );
        ");

            migrationBuilder.Sql(@"
            DELETE FROM deeplynx.events 
            WHERE project_id IS NOT NULL 
            AND NOT EXISTS (
                SELECT 1 FROM deeplynx.projects 
                WHERE id = events.project_id
            );
        ");

            // Add with CASCADE to match CascadingConstraintsForFKs
            migrationBuilder.AddForeignKey(
                name: "events_data_source_id_fkey",  // ✅ Match the name used in CascadingConstraintsForFKs
                schema: "deeplynx",
                table: "events",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);  // ✅ Add CASCADE

            migrationBuilder.AddForeignKey(
                name: "events_project_id_fkey",  // ✅ Match the name
                schema: "deeplynx",
                table: "events",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);  // ✅ Add CASCADE
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_events_data_sources_data_source_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "FK_events_projects_project_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropColumn(
                name: "entity_name",
                schema: "deeplynx",
                table: "events");
        }
    }
}
