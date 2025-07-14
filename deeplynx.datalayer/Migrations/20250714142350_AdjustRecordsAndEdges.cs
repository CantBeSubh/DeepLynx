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

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_records",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_edges",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_last_updated_at",
                schema: "deeplynx",
                table: "historical_records",
                column: "last_updated_at");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_last_updated_at",
                schema: "deeplynx",
                table: "historical_edges",
                column: "last_updated_at");
            
            migrationBuilder.DropForeignKey(
                name: "records_class_id_fkey",
                table: "records",
                schema: "deeplynx"
            );

            migrationBuilder.AddForeignKey(
                name: "records_class_id_fkey",
                table: "records",
                column: "class_id",
                schema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                principalSchema: "deeplynx",
                onDelete: ReferentialAction.SetNull
            );

            migrationBuilder.DropForeignKey(
                name: "edges_relationship_id_fkey",
                table: "edges",
                schema: "deeplynx"
            );

            migrationBuilder.AddForeignKey(
                name: "edges_relationship_id_fkey",
                table: "edges",
                column: "relationship_id",
                schema: "deeplynx",
                principalTable: "relationships",
                principalColumn: "id",
                principalSchema: "deeplynx",
                onDelete: ReferentialAction.SetNull
            );

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

            migrationBuilder.DropIndex(
                name: "idx_historical_records_last_updated_at",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropIndex(
                name: "idx_historical_edges_last_updated_at",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropForeignKey(
                name: "records_class_id_fkey",
                table: "records",
                schema: "deeplynx"
            );

            migrationBuilder.AddForeignKey(
                name: "records_class_id_fkey",
                table: "records",
                column: "class_id",
                schema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                principalSchema: "deeplynx",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.DropForeignKey(
                name: "edges_relationship_id_fkey",
                table: "edges",
                schema: "deeplynx"
            );

            migrationBuilder.AddForeignKey(
                name: "edges_relationship_id_fkey",
                table: "edges",
                column: "relationship_id",
                schema: "deeplynx",
                principalTable: "relationships",
                principalColumn: "id",
                principalSchema: "deeplynx",
                onDelete: ReferentialAction.Cascade
            );

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
