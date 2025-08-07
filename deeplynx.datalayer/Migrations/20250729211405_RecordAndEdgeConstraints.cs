using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RecordAndEdgeConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "original_id",
                schema: "deeplynx",
                table: "records",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "deeplynx",
                table: "records",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "deeplynx",
                table: "records",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "unique_record_original_id",
                schema: "deeplynx",
                table: "records",
                columns: new[] { "project_id", "data_source_id", "original_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "unique_edge_record_ids",
                schema: "deeplynx",
                table: "edges",
                columns: new[] { "project_id", "origin_id", "destination_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "unique_record_original_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "unique_edge_record_ids",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.AlterColumn<string>(
                name: "original_id",
                schema: "deeplynx",
                table: "records",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "deeplynx",
                table: "records",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "deeplynx",
                table: "records",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
