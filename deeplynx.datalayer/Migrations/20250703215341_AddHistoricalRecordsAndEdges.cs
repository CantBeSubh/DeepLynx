using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoricalRecordsAndEdges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_records_class_name",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "class_name",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "custom_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "relationship_name",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "edges",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "historical_records",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    record_id = table.Column<long>(type: "bigint", nullable: true),
                    uri = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "jsonb", nullable: false),
                    original_id = table.Column<string>(type: "text", nullable: true),
                    class_id = table.Column<long>(type: "bigint", nullable: true),
                    class_name = table.Column<string>(type: "text", nullable: true),
                    mapping_id = table.Column<long>(type: "bigint", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    data_source_name = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    project_name = table.Column<long>(type: "bigint", nullable: false),
                    tags = table.Column<string>(type: "jsonb", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("historical_records_pkey", x => x.id);
                    table.ForeignKey(
                        name: "historical_records_record_id_fkey",
                        column: x => x.record_id,
                        principalSchema: "deeplynx",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "historical_edges",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    edge_id = table.Column<long>(type: "bigint", nullable: true),
                    origin_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_id = table.Column<long>(type: "bigint", nullable: false),
                    relationship_id = table.Column<long>(type: "bigint", nullable: true),
                    relationship_name = table.Column<string>(type: "text", nullable: true),
                    mapping_id = table.Column<long>(type: "bigint", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("historical_edges_pkey", x => x.id);
                    table.ForeignKey(
                        name: "historical_edges_destination_id_fkey",
                        column: x => x.destination_id,
                        principalSchema: "deeplynx",
                        principalTable: "historical_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "historical_edges_edge_id_fkey",
                        column: x => x.edge_id,
                        principalSchema: "deeplynx",
                        principalTable: "edges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "historical_edges_origin_id_fkey",
                        column: x => x.origin_id,
                        principalSchema: "deeplynx",
                        principalTable: "historical_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_destination_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_edge_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "edge_id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_origin_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_relationship_name",
                schema: "deeplynx",
                table: "historical_edges",
                column: "relationship_name");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_class_name",
                schema: "deeplynx",
                table: "historical_records",
                column: "class_name");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_id",
                schema: "deeplynx",
                table: "historical_records",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_properties",
                schema: "deeplynx",
                table: "historical_records",
                column: "properties")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_record_id",
                schema: "deeplynx",
                table: "historical_records",
                column: "record_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historical_edges",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "historical_records",
                schema: "deeplynx");

            migrationBuilder.AddColumn<string>(
                name: "class_name",
                schema: "deeplynx",
                table: "records",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_id",
                schema: "deeplynx",
                table: "records",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "edges",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "relationship_name",
                schema: "deeplynx",
                table: "edges",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_records_class_name",
                schema: "deeplynx",
                table: "records",
                column: "class_name");
        }
    }
}
