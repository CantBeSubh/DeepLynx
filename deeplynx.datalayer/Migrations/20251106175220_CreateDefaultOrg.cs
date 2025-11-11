using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class CreateDefaultOrg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "default_org",
                schema: "deeplynx",
                table: "organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "unique_organization_name",
                schema: "deeplynx",
                table: "organizations",
                column: "name",
                unique: true);

            migrationBuilder.DropForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.Sql(@"
                DO $$
                DECLARE inl_id BIGINT;
                BEGIN
                    INSERT INTO deeplynx.organizations (name, description)
                    VALUES ('INL', 'Default Organization')
                    ON CONFLICT (name) DO NOTHING;

                    SELECT id INTO inl_id
                    FROM deeplynx.organizations
                    WHERE name = 'INL'
                    LIMIT 1;

                    -- sets INL as default org
                    UPDATE deeplynx.organizations
                    SET default_org = TRUE
                    WHERE id = inl_id;

                    -- Prevents other orgs from being default
                    UPDATE deeplynx.organizations
                    SET default_org = FALSE
                    WHERE id <> inl_id;

                    -- backfill proj orgs
                    UPDATE deeplynx.projects p
                    SET organization_id = inl_id
                    WHERE p.organization_id IS NULL;

                    IF EXISTS (SELECT 1 FROM deeplynx.projects WHERE organization_id IS NULL) THEN
                        RAISE EXCEPTION 'Backfill failed: some projects still have NULL organization_id';
                    END IF;
                END $$;
            ");
            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects",
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
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.Sql(@"
                DO $$
                DECLARE inl_id BIGINT;
                BEGIN
                    SELECT id INTO inl_id
                    FROM deeplynx.organizations
                    WHERE name = 'INL'
                    LIMIT 1;

                    IF inl_id IS NULL THEN
                    RETURN;
                    END IF;

                    UPDATE deeplynx.projects
                    SET organization_id = NULL
                    WHERE organization_id = inl_id;

                    DELETE FROM deeplynx.organizations
                    WHERE id = inl_id;
                END $$;
            ");
            migrationBuilder.DropColumn(
                name: "default_org",
                schema: "deeplynx",
                table: "organizations");

            migrationBuilder.AddForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropIndex(
                name: "unique_organization_name",
                schema: "deeplynx",
                table: "organizations");
        }
    }
}

