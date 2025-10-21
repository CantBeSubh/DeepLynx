using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class ConvertClassLastUpdatedByToBigintAndAddIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "classes");
            migrationBuilder.AddColumn<long>(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "classes",
                type: "bigint",
                nullable: true);
            migrationBuilder.CreateIndex(
                name: "idx_classes_last_updated_by",
                schema: "deeplynx",
                table: "classes",
                column: "last_updated_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
                name: "idx_classes_last_updated_by",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.AlterColumn<string>(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "classes",
                type: "text",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
