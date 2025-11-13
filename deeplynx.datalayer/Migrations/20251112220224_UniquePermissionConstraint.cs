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
                name: "permissions_unique_label_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.CreateIndex(
                name: "permissions_unique_org_label_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "organization_id", "label_id", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "permissions_unique_org_resource_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "organization_id", "resource", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "permissions_unique_project_label_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "project_id", "label_id", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "permissions_unique_project_resource_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "project_id", "resource", "action" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "permissions_unique_org_label_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_org_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_project_label_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_project_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.CreateIndex(
                name: "permissions_unique_label_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "project_id", "organization_id", "label_id", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "permissions_unique_resource_action",
                schema: "deeplynx",
                table: "permissions",
                columns: new[] { "resource", "action" },
                unique: true);
        }
    }
}
