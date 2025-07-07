using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordMappingForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "records_class_id_fkey",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.AddColumn<long>(
                name: "mapping_id",
                schema: "deeplynx",
                table: "records",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_records_mapping_id",
                schema: "deeplynx",
                table: "records",
                column: "mapping_id");

            migrationBuilder.AddForeignKey(
                name: "records_class_id_fkey",
                schema: "deeplynx",
                table: "records",
                column: "class_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "records_mapping_id_fkey",
                schema: "deeplynx",
                table: "records",
                column: "mapping_id",
                principalSchema: "deeplynx",
                principalTable: "record_mappings",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "records_class_id_fkey",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropForeignKey(
                name: "records_mapping_id_fkey",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_records_mapping_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "mapping_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.AddForeignKey(
                name: "records_class_id_fkey",
                schema: "deeplynx",
                table: "records",
                column: "class_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
