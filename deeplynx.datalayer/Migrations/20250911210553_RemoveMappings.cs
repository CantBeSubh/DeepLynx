using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "edges_mapping_id_fkey",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "records_mapping_id_fkey",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropTable(
                name: "edge_mappings",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "record_mapping_tags",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "record_mappings",
                schema: "deeplynx");

            migrationBuilder.DropIndex(
                name: "idx_records_mapping_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_edges_mapping_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "mapping_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "mapping_id",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropColumn(
                name: "mapping_id",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropColumn(
                name: "mapping_id",
                schema: "deeplynx",
                table: "edges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "mapping_id",
                schema: "deeplynx",
                table: "records",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "mapping_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "mapping_id",
                schema: "deeplynx",
                table: "historical_edges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "mapping_id",
                schema: "deeplynx",
                table: "edges",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "edge_mappings",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_id = table.Column<long>(type: "bigint", nullable: false),
                    origin_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    relationship_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_params = table.Column<string>(type: "jsonb", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    origin_params = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edge_mappings_pkey", x => x.id);
                    table.ForeignKey(
                        name: "edge_mappings_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_mappings_destination_id_fkey",
                        column: x => x.destination_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_mappings_origin_id_fkey",
                        column: x => x.origin_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_mappings_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_mappings_relationship_id_fkey",
                        column: x => x.relationship_id,
                        principalSchema: "deeplynx",
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "record_mappings",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    class_id = table.Column<long>(type: "bigint", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    record_params = table.Column<string>(type: "jsonb", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("record_mappings_pkey", x => x.id);
                    table.ForeignKey(
                        name: "record_mapping_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "record_mappings_class_id_fkey",
                        column: x => x.class_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "record_mappings_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "record_mapping_tags",
                schema: "deeplynx",
                columns: table => new
                {
                    RecordMappingId = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: false),
                    record_mapping_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("record_mapping_tags_pkey", x => new { x.RecordMappingId, x.tag_id });
                    table.ForeignKey(
                        name: "record_mapping_tags_record_mapping_id_fkey",
                        column: x => x.RecordMappingId,
                        principalSchema: "deeplynx",
                        principalTable: "record_mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "record_mapping_tags_tag_id_fkey",
                        column: x => x.tag_id,
                        principalSchema: "deeplynx",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_records_mapping_id",
                schema: "deeplynx",
                table: "records",
                column: "mapping_id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_mapping_id",
                schema: "deeplynx",
                table: "edges",
                column: "mapping_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_data_source_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_destination_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_origin_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_project_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_relationship_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mapping_tags_record_mapping_id",
                schema: "deeplynx",
                table: "record_mapping_tags",
                column: "RecordMappingId");

            migrationBuilder.CreateIndex(
                name: "idx_record_mapping_tags_tag_id",
                schema: "deeplynx",
                table: "record_mapping_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_class_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_data_source_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_project_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_tag_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "tag_id");

            migrationBuilder.AddForeignKey(
                name: "edges_mapping_id_fkey",
                schema: "deeplynx",
                table: "edges",
                column: "mapping_id",
                principalSchema: "deeplynx",
                principalTable: "edge_mappings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "records_mapping_id_fkey",
                schema: "deeplynx",
                table: "records",
                column: "mapping_id",
                principalSchema: "deeplynx",
                principalTable: "record_mappings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
