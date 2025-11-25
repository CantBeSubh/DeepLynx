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
            
            migrationBuilder.DropForeignKey(
                name: "data_sources_organization_id_fkey",
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
                name: "unique_organization_data_source_name",
                schema: "deeplynx",
                table: "data_sources",
                columns: new[] { "organization_id", "name" },
                unique: true,
                filter: "project_id IS NULL");
            
            migrationBuilder.CreateIndex(
                name: "unique_project_data_source_name",
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
            
            migrationBuilder.AddForeignKey(
                name: "data_sources_organization_id_fkey",
                schema: "deeplynx",
                table: "data_sources",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
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
            
            migrationBuilder.DropForeignKey(
                name: "data_sources_organization_id_fkey",
                schema: "deeplynx",
                table: "data_sources");
            
            migrationBuilder.DropIndex(
                name: "unique_organization_data_source_name",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropIndex(
                name: "unique_project_data_source_name",
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
                principalColumn: "id");
            
            migrationBuilder.AddForeignKey(
                name: "data_sources_organization_id_fkey",
                schema: "deeplynx",
                table: "data_sources",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id");
        }
    }
}
