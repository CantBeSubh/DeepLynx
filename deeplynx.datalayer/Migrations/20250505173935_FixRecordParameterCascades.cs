using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixRecordParameterCascades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "record_parameters_tag_id_fkey",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropForeignKey(
                name: "record_parameters_class_id_fkey",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.AddForeignKey(
                name: "record_parameters_tag_id_fkey",
                schema: "deeplynx",
                table: "record_parameters",
                column: "tag_id",
                principalSchema: "deeplynx",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "record_parameters_class_id_fkey",
                schema: "deeplynx",
                table: "record_parameters",
                column: "class_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "record_parameters_tag_id_fkey",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropForeignKey(
                name: "record_parameters_class_id_fkey",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.AddForeignKey(
                name: "record_parameters_tag_id_fkey",
                schema: "deeplynx",
                table: "record_parameters",
                column: "tag_id",
                principalSchema: "deeplynx",
                principalTable: "tags",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "record_parameters_class_id_fkey",
                schema: "deeplynx",
                table: "record_parameters",
                column: "class_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id");
        }
    }
}
