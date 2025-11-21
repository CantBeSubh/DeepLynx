using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgToTags : Migration
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

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'deeplynx'
                        AND table_name = 'tags'
                        AND column_name = 'organization_id'
                    ) THEN
                        ALTER TABLE deeplynx.tags ADD COLUMN organization_id bigint NULL;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.tags t
                SET organization_id = p.organization_id
                FROM deeplynx.projects p
                WHERE t.project_id = p.id
                AND t.organization_id IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.tags t
                SET organization_id = o.id
                FROM deeplynx.organizations o
                WHERE o.name = 'default_org'
                AND t.organization_id IS NULL;
            ");

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "tags",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

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
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "tags_project_id_fkey",
                schema: "deeplynx",
                table: "tags",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "tags_organization_id_fkey",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "tags_project_id_fkey",
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

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "tags",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "tags_organization_id_fkey",
                schema: "deeplynx",
                table: "tags",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "tags_project_id_fkey",
                schema: "deeplynx",
                table: "tags",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
