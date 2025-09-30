using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RoleNameConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
