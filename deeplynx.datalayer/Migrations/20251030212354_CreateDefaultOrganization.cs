// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace deeplynx.datalayer.Migrations
// {
//     /// <inheritdoc />
//     public partial class CreateDefaultOrganization : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "projects_organization_id_fkey",
//                 schema: "deeplynx",
//                 table: "projects");

//             migrationBuilder.AlterColumn<long>(
//                 name: "organization_id",
//                 schema: "deeplynx",
//                 table: "projects",
//                 type: "bigint",
//                 nullable: false,
//                 defaultValue: 0L,
//                 oldClrType: typeof(long),
//                 oldType: "bigint",
//                 oldNullable: true);

//             migrationBuilder.AddForeignKey(
//                 name: "projects_organization_id_fkey",
//                 schema: "deeplynx",
//                 table: "projects",
//                 column: "organization_id",
//                 principalSchema: "deeplynx",
//                 principalTable: "organizations",
//                 principalColumn: "id",
//                 onDelete: ReferentialAction.Restrict);
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "projects_organization_id_fkey",
//                 schema: "deeplynx",
//                 table: "projects");

//             migrationBuilder.AlterColumn<long>(
//                 name: "organization_id",
//                 schema: "deeplynx",
//                 table: "projects",
//                 type: "bigint",
//                 nullable: true,
//                 oldClrType: typeof(long),
//                 oldType: "bigint");

//             migrationBuilder.AddForeignKey(
//                 name: "projects_organization_id_fkey",
//                 schema: "deeplynx",
//                 table: "projects",
//                 column: "organization_id",
//                 principalSchema: "deeplynx",
//                 principalTable: "organizations",
//                 principalColumn: "id",
//                 onDelete: ReferentialAction.SetNull);
//         }
//     }
// }

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class CreateDefaultOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Ensure default org "INL" exists, then backfill NULL project orgs
            migrationBuilder.Sql(@"
            DO $$
            DECLARE inl_id BIGINT;
            BEGIN
              INSERT INTO deeplynx.organizations (name, is_archived, created_at, last_updated_at)
              VALUES ('INL', FALSE, NOW(), NOW())
              ON CONFLICT (name) DO NOTHING;

              SELECT id INTO inl_id FROM deeplynx.organizations WHERE name = 'INL' LIMIT 1;

              UPDATE deeplynx.projects p
              SET organization_id = inl_id
              WHERE p.organization_id IS NULL;

              -- Guardrail: fail the migration if any NULLs remain
              IF EXISTS (SELECT 1 FROM deeplynx.projects WHERE organization_id IS NULL) THEN
                RAISE EXCEPTION 'Backfill failed: some projects still have NULL organization_id';
              END IF;
            END $$;
            ");

            // 2) Enforce NOT NULL on projects.organization_id
            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            // 3) Index and FK (Restrict delete)
            migrationBuilder.CreateIndex(
                name: "ix_projects_organization_id",
                schema: "deeplynx",
                table: "projects",
                column: "organization_id");

            migrationBuilder.AddForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse schema
            migrationBuilder.DropForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "ix_projects_organization_id",
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
        }
    }
}
