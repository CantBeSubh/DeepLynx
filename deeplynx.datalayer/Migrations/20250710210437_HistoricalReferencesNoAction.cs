using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class HistoricalReferencesNoAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "historical_records_record_id_fkey",
                table: "historical_records",
                schema: "deeplynx"
            );

            migrationBuilder.AddForeignKey(
                name: "historical_records_record_id_fkey",
                table: "historical_records",
                column: "record_id",
                schema: "deeplynx",
                principalTable: "records",
                principalColumn: "id",
                principalSchema: "deeplynx",
                onDelete: ReferentialAction.NoAction
            );

            migrationBuilder.DropForeignKey(
                name: "historical_edges_edge_id_fkey",
                table: "historical_edges",
                schema: "deeplynx"
            );

            migrationBuilder.AddForeignKey(
                name: "historical_edges_edge_id_fkey",
                table: "historical_edges",
                column: "edge_id",
                schema: "deeplynx",
                principalTable: "edges",
                principalColumn: "id",
                principalSchema: "deeplynx",
                onDelete: ReferentialAction.NoAction
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "historical_records_record_id_fkey",
                table: "historical_records",
                schema: "deeplynx"
            );

            migrationBuilder.AddForeignKey(
                name: "historical_records_record_id_fkey",
                table: "historical_records",
                column: "record_id",
                schema: "deeplynx",
                principalTable: "records",
                principalColumn: "id",
                principalSchema: "deeplynx",
                onDelete: ReferentialAction.SetNull
            );

            migrationBuilder.DropForeignKey(
                name: "historical_edges_edge_id_fkey",
                table: "historical_edges",
                schema: "deeplynx"
            );

            migrationBuilder.AddForeignKey(
                name: "historical_edges_edge_id_fkey",
                table: "historical_edges",
                column: "edge_id",
                schema: "deeplynx",
                principalTable: "edges",
                principalColumn: "id",
                principalSchema: "deeplynx",
                onDelete: ReferentialAction.SetNull
            );
        }
    }
}
