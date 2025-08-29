using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectOrganizationFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // due to existing data in the projects table, do the following:
            // 1. set nullable on projects.organization_id to true
            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects",
                type: "bigint",
                nullable: true);

            // 2. create a default organization for the application instance
            // 3. update existing projects with the returned default organization_id
            migrationBuilder.Sql(@"
                WITH inserted AS (
                    INSERT INTO deeplynx.organizations (name) VALUES ('Default Organization')
                    RETURNING id)
                UPDATE deeplynx.projects SET organization_id = (SELECT id FROM inserted) WHERE organization_id IS NULL;
            ");

            // 4. Now that every project has an organization_id, make it non-nullable
            migrationBuilder.AlterColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true
            );

            // 5. Add a foreign key constraint on organization_id
            migrationBuilder.AddForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            // 6. Add an index on the foreign key column organization_id
            migrationBuilder.CreateIndex(
                name: "idx_projects_organization_id",
                schema: "deeplynx",
                table: "projects",
                column: "organization_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "idx_projects_organization_id",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects");
        }
    }
}
