using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RelationshipProjectIdNotRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "relationships_project_id_fkey",
                schema: "deeplynx",
                table: "relationships");
            
            migrationBuilder.DropForeignKey(
                name: "relationships_organization_id_fkey",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "relationships",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "unique_organization_relationship_name",
                schema: "deeplynx",
                table: "relationships",
                columns: new[] { "organization_id", "name" },
                unique: true,
                filter: "project_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "unique_project_relationship_name",
                schema: "deeplynx",
                table: "relationships",
                columns: new[] { "organization_id", "project_id", "name" },
                unique: true,
                filter: "project_id IS NOT NULL");

            // Re-add foreign key constraints with CASCADE delete
            migrationBuilder.AddForeignKey(
                name: "relationships_project_id_fkey",
                schema: "deeplynx",
                table: "relationships",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.AddForeignKey(
                name: "relationships_organization_id_fkey",
                schema: "deeplynx",
                table: "relationships",
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
                name: "relationships_project_id_fkey",
                schema: "deeplynx",
                table: "relationships");
            
            migrationBuilder.DropForeignKey(
                name: "relationships_organization_id_fkey",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "unique_organization_relationship_name",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "unique_project_relationship_name",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "relationships",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            // Re-add foreign key constraints with original behavior (likely NO ACTION or RESTRICT)
            migrationBuilder.AddForeignKey(
                name: "relationships_project_id_fkey",
                schema: "deeplynx",
                table: "relationships",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");
            
            migrationBuilder.AddForeignKey(
                name: "relationships_organization_id_fkey",
                schema: "deeplynx",
                table: "relationships",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id");
        }
    }
}
