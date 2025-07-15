using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixHistoricalFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "record_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "edge_id",
                schema: "deeplynx",
                table: "historical_edges",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "historical_edges_edge_id_fkey",
                schema: "deeplynx",
                table: "historical_edges",
                column: "edge_id",
                principalSchema: "deeplynx",
                principalTable: "edges",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "historical_records_record_id_fkey",
                schema: "deeplynx",
                table: "historical_records",
                column: "record_id",
                principalSchema: "deeplynx",
                principalTable: "records",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "historical_edges_edge_id_fkey",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropForeignKey(
                name: "historical_records_record_id_fkey",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.AlterColumn<long>(
                name: "record_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "edge_id",
                schema: "deeplynx",
                table: "historical_edges",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "historical_edges_edge_id_fkey",
                schema: "deeplynx",
                table: "historical_edges",
                column: "edge_id",
                principalSchema: "deeplynx",
                principalTable: "edges",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "historical_records_record_id_fkey",
                schema: "deeplynx",
                table: "historical_records",
                column: "record_id",
                principalSchema: "deeplynx",
                principalTable: "records",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
