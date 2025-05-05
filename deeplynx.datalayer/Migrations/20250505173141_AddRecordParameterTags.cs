using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordParameterTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "record_parameters_class_id_fkey",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.AlterColumn<long>(
                name: "class_id",
                schema: "deeplynx",
                table: "record_parameters",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "tag_id",
                schema: "deeplynx",
                table: "record_parameters",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_record_parameters_tag_id",
                schema: "deeplynx",
                table: "record_parameters",
                column: "tag_id");

            migrationBuilder.AddForeignKey(
                name: "record_parameters_class_id_fkey",
                schema: "deeplynx",
                table: "record_parameters",
                column: "class_id",
                principalSchema: "deeplynx",
                principalTable: "classes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "record_parameters_tag_id_fkey",
                schema: "deeplynx",
                table: "record_parameters",
                column: "tag_id",
                principalSchema: "deeplynx",
                principalTable: "tags",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "record_parameters_class_id_fkey",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropForeignKey(
                name: "record_parameters_tag_id_fkey",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropIndex(
                name: "idx_record_parameters_tag_id",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropColumn(
                name: "tag_id",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.AlterColumn<long>(
                name: "class_id",
                schema: "deeplynx",
                table: "record_parameters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

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
    }
}
