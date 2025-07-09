using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AdjustRecordsAndEdges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "historical_edges_destination_id_fkey",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropForeignKey(
                name: "historical_edges_origin_id_fkey",
                schema: "deeplynx",
                table: "historical_edges");

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

            migrationBuilder.AlterColumn<string>(
                name: "project_name",
                schema: "deeplynx",
                table: "historical_records",
                type: "text",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "data_source_name",
                schema: "deeplynx",
                table: "historical_records",
                type: "text",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<bool>(
                name: "current",
                schema: "deeplynx",
                table: "historical_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AddColumn<bool>(
                name: "current",
                schema: "deeplynx",
                table: "historical_edges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_current",
                schema: "deeplynx",
                table: "historical_records",
                column: "current");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_current",
                schema: "deeplynx",
                table: "historical_edges",
                column: "current");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_historical_records_current",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropIndex(
                name: "idx_historical_edges_current",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropColumn(
                name: "current",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropColumn(
                name: "current",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.AlterColumn<long>(
                name: "record_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "project_name",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "data_source_name",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "edge_id",
                schema: "deeplynx",
                table: "historical_edges",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "historical_edges_destination_id_fkey",
                schema: "deeplynx",
                table: "historical_edges",
                column: "destination_id",
                principalSchema: "deeplynx",
                principalTable: "historical_records",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "historical_edges_origin_id_fkey",
                schema: "deeplynx",
                table: "historical_edges",
                column: "origin_id",
                principalSchema: "deeplynx",
                principalTable: "historical_records",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
