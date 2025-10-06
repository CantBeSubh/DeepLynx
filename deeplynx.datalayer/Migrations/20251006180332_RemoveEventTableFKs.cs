using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEventTableFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "events_dataSource_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.RenameIndex(
                name: "IX_events_data_source_id",
                schema: "deeplynx",
                table: "events",
                newName: "idx_events_data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_events_organization_id",
                schema: "deeplynx",
                table: "events",
                column: "organization_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_events_organization_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.RenameIndex(
                name: "idx_events_data_source_id",
                schema: "deeplynx",
                table: "events",
                newName: "IX_events_data_source_id");

            migrationBuilder.AddForeignKey(
                name: "events_dataSource_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");
        }
    }
}
