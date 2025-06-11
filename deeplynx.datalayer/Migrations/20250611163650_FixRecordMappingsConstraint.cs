using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixRecordMappingsConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "record_mappings_class_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");
            
            migrationBuilder.AddForeignKey(
                name: "record_mappings_class_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
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
                name: "record_mappings_class_id_fkey",
                schema: "deeplynx",
                table: "record_mappings");
            
            migrationBuilder.AddForeignKey(
                name: "record_mappings_class_id_fkey",
                schema: "deeplynx",
                table: "record_mappings",
                column: "class_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id");
        }
    }
}
