using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeParamsToMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "record_parameters",
                schema: "deeplynx",
                newName: "record_mappings",
                newSchema: "deeplynx");

            migrationBuilder.RenameTable(
                name: "edge_parameters",
                schema: "deeplynx",
                newName: "edge_mappings",
                newSchema: "deeplynx");

            migrationBuilder.RenameIndex(
                name: "idx_record_parameters_class_id",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "idx_record_mappings_class_id");

            migrationBuilder.RenameIndex(
                name: "idx_record_parameters_tag_id",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "idx_record_mappings_tag_id");

            migrationBuilder.RenameIndex(
                name: "idx_record_parameters_project_id",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "idx_record_mappings_project_id");

            migrationBuilder.RenameIndex(
                name: "idx_record_parameters_id",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "idx_record_mappings_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_parameters_relationship_id",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "idx_edge_mappings_relationship_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_parameters_project_id",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "idx_edge_mappings_project_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_parameters_origin_id",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "idx_edge_mappings_origin_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_parameters_destination_id",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "idx_edge_mappings_destination_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_parameters_id",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "idx_edge_mappings_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "record_mappings",
                schema: "deeplynx",
                newName: "record_parameters",
                newSchema: "deeplynx");

            migrationBuilder.RenameTable(
                name: "edge_mappings",
                schema: "deeplynx",
                newName: "edge_parameters",
                newSchema: "deeplynx");

            migrationBuilder.RenameIndex(
                name: "idx_record_mappings_class_id",
                schema: "deeplynx",
                table: "record_parameters",
                newName: "idx_record_parameters_class_id");

            migrationBuilder.RenameIndex(
                name: "idx_record_mappings_tag_id",
                schema: "deeplynx",
                table: "record_parameters",
                newName: "idx_record_parameters_tag_id");

            migrationBuilder.RenameIndex(
                name: "idx_record_mappings_project_id",
                schema: "deeplynx",
                table: "record_parameters",
                newName: "idx_record_parameters_project_id");

            migrationBuilder.RenameIndex(
                name: "idx_record_mappings_id",
                schema: "deeplynx",
                table: "record_parameters",
                newName: "idx_record_parameters_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_mappings_relationship_id",
                schema: "deeplynx",
                table: "edge_parameters",
                newName: "idx_edge_parameters_relationship_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_mappings_project_id",
                schema: "deeplynx",
                table: "edge_parameters",
                newName: "idx_edge_parameters_project_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_mappings_origin_id",
                schema: "deeplynx",
                table: "edge_parameters",
                newName: "idx_edge_parameters_origin_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_mappings_destination_id",
                schema: "deeplynx",
                table: "edge_parameters",
                newName: "idx_edge_parameters_destination_id");

            migrationBuilder.RenameIndex(
                name: "idx_edge_mappings_id",
                schema: "deeplynx",
                table: "edge_parameters",
                newName: "idx_edge_parameters_id");
        }
    }
}
