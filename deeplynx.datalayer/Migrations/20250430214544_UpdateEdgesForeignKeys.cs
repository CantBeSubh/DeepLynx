using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEdgesForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "data_source_id",
                schema: "deeplynx",
                table: "edges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "edges",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "idx_edges_data_source_id",
                schema: "deeplynx",
                table: "edges",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_project_id",
                schema: "deeplynx",
                table: "edges",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "edges_data_source_id_fkey",
                schema: "deeplynx",
                table: "edges",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "edges_project_id_fkey",
                schema: "deeplynx",
                table: "edges",
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
                name: "edges_data_source_id_fkey",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "edges_project_id_fkey",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "idx_edges_data_source_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "idx_edges_project_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "data_source_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "project_id",
                schema: "deeplynx",
                table: "edges");
        }
    }
}
