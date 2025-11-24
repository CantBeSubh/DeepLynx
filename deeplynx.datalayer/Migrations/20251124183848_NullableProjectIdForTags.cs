using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class NullableProjectIdForTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "tags_organization_id_fkey",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "tags_project_id_fkey",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "tags",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "unique_organization_tag_name",
                schema: "deeplynx",
                table: "tags",
                columns: new[] { "organization_id", "name" },
                unique: true,
                filter: "project_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "unique_project_tag_name",
                schema: "deeplynx",
                table: "tags",
                columns: new[] { "organization_id", "project_id", "name" },
                unique: true,
                filter: "project_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "tags_organization_id_fkey",
                schema: "deeplynx",
                table: "tags",
                column: "last_updated_by",
                principalSchema: "deeplynx",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "tags_project_id_fkey",
                schema: "deeplynx",
                table: "tags",
                column: "last_updated_by",
                principalSchema: "deeplynx",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "tags_project_id_fkey",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "tags_organization_id_fkey",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "unique_organization_tag_name",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "unique_project_tag_name",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "tags",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
