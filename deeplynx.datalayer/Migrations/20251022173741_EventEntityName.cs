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
