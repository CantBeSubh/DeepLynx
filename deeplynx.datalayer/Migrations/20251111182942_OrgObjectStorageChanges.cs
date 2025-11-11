using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class OrgObjectStorageChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "object_storages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "object_storages",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_object_storages_organization_id",
                schema: "deeplynx",
                table: "object_storages",
                column: "organization_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_object_storages_ProjectXorOrg",
                schema: "deeplynx",
                table: "object_storages",
                sql: "(project_id IS NOT NULL AND organization_id IS NULL) OR (project_id IS NULL AND organization_id IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_object_storages_organizations_organization_id",
                schema: "deeplynx",
                table: "object_storages",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_object_storages_organizations_organization_id",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropIndex(
                name: "IX_object_storages_organization_id",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropCheckConstraint(
                name: "ck_object_storages_ProjectXorOrg",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "object_storages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "object_storage_project_id_fkey",
                schema: "deeplynx",
                table: "object_storages",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
