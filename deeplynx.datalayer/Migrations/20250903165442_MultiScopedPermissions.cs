using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class MultiScopedPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "sensitivity_labels_organization_id_fkey",
                schema: "deeplynx",
                table: "sensitivity_labels");

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "sensitivity_labels",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "sensitivity_labels",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "roles",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "roles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_sensitivity_labels_project_id",
                schema: "deeplynx",
                table: "sensitivity_labels",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_roles_organization_id",
                schema: "deeplynx",
                table: "roles",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_action",
                schema: "deeplynx",
                table: "permissions",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_domain",
                schema: "deeplynx",
                table: "permissions",
                column: "domain");

            migrationBuilder.AddForeignKey(
                name: "roles_organization_id_fkey",
                schema: "deeplynx",
                table: "roles",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "sensitivity_label_organization_id_fkey",
                schema: "deeplynx",
                table: "sensitivity_labels",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "sensitivity_label_project_id_fkey",
                schema: "deeplynx",
                table: "sensitivity_labels",
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
                name: "roles_organization_id_fkey",
                schema: "deeplynx",
                table: "roles");

            migrationBuilder.DropForeignKey(
                name: "sensitivity_label_organization_id_fkey",
                schema: "deeplynx",
                table: "sensitivity_labels");

            migrationBuilder.DropForeignKey(
                name: "sensitivity_label_project_id_fkey",
                schema: "deeplynx",
                table: "sensitivity_labels");

            migrationBuilder.DropIndex(
                name: "idx_sensitivity_labels_project_id",
                schema: "deeplynx",
                table: "sensitivity_labels");

            migrationBuilder.DropIndex(
                name: "idx_roles_organization_id",
                schema: "deeplynx",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "idx_permissions_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "idx_permissions_domain",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "project_id",
                schema: "deeplynx",
                table: "sensitivity_labels");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "roles");

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "sensitivity_labels",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "roles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "sensitivity_labels_organization_id_fkey",
                schema: "deeplynx",
                table: "sensitivity_labels",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
