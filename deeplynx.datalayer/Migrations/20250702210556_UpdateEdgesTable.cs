using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEdgesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "properties",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.AddColumn<long>(
                name: "mapping_id",
                schema: "deeplynx",
                table: "edges",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_edges_mapping_id",
                schema: "deeplynx",
                table: "edges",
                column: "mapping_id");

            migrationBuilder.AddForeignKey(
                name: "edges_mapping_id_fkey",
                schema: "deeplynx",
                table: "edges",
                column: "mapping_id",
                principalSchema: "deeplynx",
                principalTable: "edge_mappings",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "edges_mapping_id_fkey",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "idx_edges_mapping_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "mapping_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.AddColumn<string>(
                name: "properties",
                schema: "deeplynx",
                table: "edges",
                type: "jsonb",
                nullable: true);
        }
    }
}
