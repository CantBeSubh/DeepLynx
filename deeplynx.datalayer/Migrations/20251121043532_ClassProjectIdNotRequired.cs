using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class ClassProjectIdNotRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "classes_project_id_fkey",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropForeignKey(
                name: "classes_organization_id_fkey",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "classes",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "unique_organization_class_name",
                schema: "deeplynx",
                table: "classes",
                columns: new[] { "organization_id", "name" },
                unique: true,
                filter: "project_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "unique_project_class_name",
                schema: "deeplynx",
                table: "classes",
                columns: new[] { "organization_id", "project_id", "name" },
                unique: true,
                filter: "project_id IS NOT NULL");

            // Re-add foreign key constraints with CASCADE delete
            migrationBuilder.AddForeignKey(
                name: "classes_project_id_fkey",
                schema: "deeplynx",
                table: "classes",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "classes_organization_id_fkey",
                schema: "deeplynx",
                table: "classes",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop CASCADE foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "classes_project_id_fkey",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropForeignKey(
                name: "classes_organization_id_fkey",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "unique_organization_class_name",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "unique_project_class_name",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "classes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            // Re-add foreign key constraints with original behavior (likely NO ACTION or RESTRICT)
            migrationBuilder.AddForeignKey(
                name: "classes_project_id_fkey",
                schema: "deeplynx",
                table: "classes",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "classes_organization_id_fkey",
                schema: "deeplynx",
                table: "classes",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id");
        }
    }
}