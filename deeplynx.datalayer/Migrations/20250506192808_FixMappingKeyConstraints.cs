using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixMappingKeyConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "edge_parameters_destination_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropForeignKey(
                name: "edge_parameters_origin_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropForeignKey(
                name: "edge_parameters_project_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropForeignKey(
                name: "edge_parameters_relationship_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropForeignKey(
                name: "record_parameters_class_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropForeignKey(
                name: "record_parameters_project_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropForeignKey(
                name: "record_parameters_tag_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropPrimaryKey(
                name: "record_parameters_pkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropPrimaryKey(
                name: "edge_parameters_pkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.AddPrimaryKey(
                name: "record_mappings_pkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "edge_mappings_pkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "edge_mappings_destination_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "destination_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "edge_mappings_origin_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "origin_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "edge_mappings_project_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "edge_mappings_relationship_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "relationship_id",
                principalSchema: "deeplynx",
                principalTable: "relationships",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "record_mappings_class_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "class_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "record_mappings_project_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "record_mappings_tag_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "tag_id",
                principalSchema: "deeplynx",
                principalTable: "tags",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "edge_mappings_destination_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropForeignKey(
                name: "edge_mappings_origin_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropForeignKey(
                name: "edge_mappings_project_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropForeignKey(
                name: "edge_mappings_relationship_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropForeignKey(
                name: "record_mappings_class_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropForeignKey(
                name: "record_mappings_project_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropForeignKey(
                name: "record_mappings_tag_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropPrimaryKey(
                name: "record_mappings_pkey",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropPrimaryKey(
                name: "edge_mappings_pkey",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.AddPrimaryKey(
                name: "record_parameters_pkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "edge_parameters_pkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "edge_parameters_destination_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "destination_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "edge_parameters_origin_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "origin_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "edge_parameters_project_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "edge_parameters_relationship_id_fkey",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "relationship_id",
                principalSchema: "deeplynx",
                principalTable: "relationships",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "record_parameters_class_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "class_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "record_parameters_project_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "record_parameters_tag_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "tag_id",
                principalSchema: "deeplynx",
                principalTable: "tags",
                principalColumn: "id");
        }
    }
}
