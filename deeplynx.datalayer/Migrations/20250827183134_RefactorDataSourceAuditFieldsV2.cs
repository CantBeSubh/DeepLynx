using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDataSourceAuditFieldsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add new is_archived column
            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "data_sources",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Step 2: Transform existing data BEFORE renaming columns
            migrationBuilder.Sql(@"
                UPDATE deeplynx.data_sources
                SET is_archived = true
                WHERE archived_at IS NOT NULL;");

            // Step 3: Update created_at with most recent timestamp
            migrationBuilder.Sql(@"
                UPDATE deeplynx.data_sources
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );");

            // Step 4: Update modified_by with most recent user
            migrationBuilder.Sql(@"
                UPDATE deeplynx.data_sources
                SET modified_by = COALESCE(modified_by, created_by);");

            // Step 5: Drop old columns
            migrationBuilder.DropColumn(
                name: "archived_at",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropColumn(
                name: "modified_at",
                schema: "deeplynx",
                table: "data_sources");

            // Step 6: Rename columns
            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "data_sources",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "data_sources",
                newName: "last_updated_at");

            // Step 7: Add indexes for new audit fields
            migrationBuilder.CreateIndex(
                name: "idx_data_sources_is_archived",
                schema: "deeplynx",
                table: "data_sources",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_data_sources_last_updated_at",
                schema: "deeplynx",
                table: "data_sources",
                column: "last_updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "idx_data_sources_is_archived",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropIndex(
                name: "idx_data_sources_last_updated_at",
                schema: "deeplynx",
                table: "data_sources");

            // Drop new column
            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "data_sources");

            // Rename columns back
            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "data_sources",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "data_sources",
                newName: "created_at");

            // Re-add old columns
            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "data_sources",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "data_sources",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "data_sources",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}