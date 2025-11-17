using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredRoleIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.unique_organization_role_name;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.unique_project_role_name;");
            
            migrationBuilder.Sql("UPDATE deeplynx.roles SET organization_id = 1 WHERE organization_id IS NULL;");

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "roles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "unique_organization_role_name",
                schema: "deeplynx",
                table: "roles",
                columns: new[] { "organization_id", "name" },
                unique: true,
                filter: "project_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "unique_project_role_name",
                schema: "deeplynx",
                table: "roles",
                columns: new[] { "organization_id", "project_id", "name" },
                unique: true,
                filter: "project_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "unique_organization_role_name",
                schema: "deeplynx",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "unique_project_role_name",
                schema: "deeplynx",
                table: "roles");

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "roles",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "unique_organization_role_name",
                schema: "deeplynx",
                table: "roles",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "unique_project_role_name",
                schema: "deeplynx",
                table: "roles",
                columns: new[] { "project_id", "name" },
                unique: true);
        }
    }
}
