using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNameConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_tags_name",
                schema: "deeplynx",
                table: "tags",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "unique_tag_name",
                schema: "deeplynx",
                table: "tags",
                columns: new[] { "project_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_relationships_name",
                schema: "deeplynx",
                table: "relationships",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "unique_relationship_name",
                schema: "deeplynx",
                table: "relationships",
                columns: new[] { "project_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_classes_name",
                schema: "deeplynx",
                table: "classes",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "unique_class_name",
                schema: "deeplynx",
                table: "classes",
                columns: new[] { "project_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_tags_name",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "unique_tag_name",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "idx_relationships_name",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "unique_relationship_name",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "idx_classes_name",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "unique_class_name",
                schema: "deeplynx",
                table: "classes");
        }
    }
}
