using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UniquePermissionConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "permissions_unique_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.CreateIndex(
                name: "permissions_unique_org_project_resource_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "organization_id", "project_id", "resource", "action" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "permissions_unique_org_project_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.CreateIndex(
                name: "permissions_unique_resource_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "resource", "action" },
                unique: true);
        }
    }
}
