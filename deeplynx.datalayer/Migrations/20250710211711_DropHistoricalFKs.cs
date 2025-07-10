using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class DropHistoricalFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "historical_records_record_id_fkey",
                table: "historical_records",
                schema: "deeplynx"
            );

            migrationBuilder.DropForeignKey(
                name: "historical_edges_edge_id_fkey",
                table: "historical_edges",
                schema: "deeplynx"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "historical_records_record_id_fkey",
                table: "historical_records",
                column: "record_id",
                principalTable: "records",
                principalColumn: "id",
                schema: "deeplynx",
                onDelete: ReferentialAction.NoAction
            );

            migrationBuilder.AddForeignKey(
                name: "historical_edges_edge_id_fkey",
                table: "historical_edges",
                column: "edge_id",
                principalTable: "edges",
                principalColumn: "id",
                schema: "deeplynx",
                onDelete: ReferentialAction.NoAction
            );
        }
    }
}
