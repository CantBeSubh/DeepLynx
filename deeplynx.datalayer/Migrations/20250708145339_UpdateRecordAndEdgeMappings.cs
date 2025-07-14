using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecordAndEdgeMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "record_mappings_tag_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.AddColumn<long>(
                name: "data_source_id",
                schema: "deeplynx",
                table: "record_mappings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "data_source_id",
                schema: "deeplynx",
                table: "edge_mappings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

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
                name: "idx_record_mappings_data_source_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_data_source_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "data_source_id");

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

            migrationBuilder.AddForeignKey(
                name: "edge_mappings_data_source_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "record_mapping_data_source_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "edge_mappings_data_source_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropForeignKey(
                name: "record_mapping_data_source_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropTable(
                name: "record_mapping_tags",
                schema: "deeplynx");

            migrationBuilder.DropIndex(
                name: "idx_record_mappings_data_source_id",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropIndex(
                name: "idx_edge_mappings_data_source_id",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropColumn(
                name: "data_source_id",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropColumn(
                name: "data_source_id",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.AddForeignKey(
                name: "record_mappings_tag_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "tag_id",
                principalSchema: "deeplynx",
                principalTable: "tags",
                principalColumn: "id");
        }
    }
}
