using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class DataSourcesProjectIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "data_sources_project_id_fkey",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "data_sources",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
            
            migrationBuilder.CreateIndex(
                name: "idx_data_sources_org_name_unique",
                schema: "deeplynx",
                table: "data_sources",
                columns: new[] { "organization_id", "name" },
                unique: true,
                filter: "project_id IS NULL");

            // Unique when project-scoped
            migrationBuilder.CreateIndex(
                name: "idx_data_sources_org_project_name_unique",
                schema: "deeplynx",
                table: "data_sources",
                columns: new[] { "organization_id", "project_id", "name" },
                unique: true,
                filter: "project_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "data_sources_project_id_fkey",
                schema: "deeplynx",
                table: "data_sources",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "data_sources_project_id_fkey",
                schema: "deeplynx",
                table: "data_sources");
            
            migrationBuilder.DropIndex(
                name: "idx_data_sources_org_name_unique",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropIndex(
                name: "idx_data_sources_org_project_name_unique",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "data_sources",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "data_sources_project_id_fkey",
                schema: "deeplynx",
                table: "data_sources",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade); //no-opted with above up application
        }
    }
}
