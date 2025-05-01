using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class DropExtraneousIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_classes_project_id",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "IX_data_sources_project_id",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropIndex(
                name: "IX_edge_parameters_destination_id",
                schema: "deeplynx",
                table: "edge_parameters");

            migrationBuilder.DropIndex(
                name: "IX_edge_parameters_origin_id",
                schema: "deeplynx",
                table: "edge_parameters");

            migrationBuilder.DropIndex(
                name: "IX_edge_parameters_project_id",
                schema: "deeplynx",
                table: "edge_parameters");

            migrationBuilder.DropIndex(
                name: "IX_edge_parameters_relationship_id",
                schema: "deeplynx",
                table: "edge_parameters");

            migrationBuilder.DropIndex(
                name: "IX_edges_destination_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "IX_edges_relationship_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "IX_record_parameters_class_id",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropIndex(
                name: "IX_records_class_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "IX_records_data_source_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "IX_records_project_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "IX_relationships_destination_id",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "IX_relationships_origin_id",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "IX_relationships_project_id",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "IX_roles_project_id",
                schema: "deeplynx",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "IX_user_projects_project_id",
                schema: "deeplynx",
                table: "user_projects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_classes_project_id",
                schema: "deeplynx",
                table: "classes",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_sources_project_id",
                schema: "deeplynx",
                table: "data_sources",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_edge_parameters_destination_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "IX_edge_parameters_origin_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "IX_edge_parameters_project_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_edge_parameters_relationship_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_edges_destination_id",
                schema: "deeplynx",
                table: "edges",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "IX_edges_relationship_id",
                schema: "deeplynx",
                table: "edges",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_parameters_class_id",
                schema: "deeplynx",
                table: "record_parameters",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_records_class_id",
                schema: "deeplynx",
                table: "records",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_records_data_source_id",
                schema: "deeplynx",
                table: "records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "IX_records_project_id",
                schema: "deeplynx",
                table: "records",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_destination_id",
                schema: "deeplynx",
                table: "relationships",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_origin_id",
                schema: "deeplynx",
                table: "relationships",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_project_id",
                schema: "deeplynx",
                table: "relationships",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_project_id",
                schema: "deeplynx",
                table: "roles",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_projects_project_id",
                schema: "deeplynx",
                table: "user_projects",
                column: "project_id");
        }
    }
}
