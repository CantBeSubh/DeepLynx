using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RefactorClassAuditFields : Migration
    {
        /// <inheritdoc />
   protected override void Up(MigrationBuilder migrationBuilder)
{
    // Step 1: Add new is_archived column
    migrationBuilder.AddColumn<bool>(
        name: "is_archived",
        schema: "deeplynx",
        table: "classes",
        type: "boolean",
        nullable: false,
        defaultValue: false);

    // Step 2: Transform existing data BEFORE renaming columns
    // Rule: is_archived = true if archived_at was NOT NULL
    migrationBuilder.Sql(@"
        UPDATE deeplynx.classes
        SET is_archived = true
        WHERE archived_at IS NOT NULL;");

    // Step 3: Update created_at with most recent timestamp (before rename)
    // Rule: created_at = most recent of created_at, modified_at, archived_at
    migrationBuilder.Sql(@"
        UPDATE deeplynx.classes
        SET created_at = GREATEST(
            COALESCE(created_at, '1900-01-01'::timestamp),
            COALESCE(modified_at, '1900-01-01'::timestamp),
            COALESCE(archived_at, '1900-01-01'::timestamp)
        );");

    // Step 4: Update modified_by with most recent user (before rename)
    // Rule: modified_by = modified_by if exists, otherwise created_by
    migrationBuilder.Sql(@"
        UPDATE deeplynx.classes
        SET modified_by = COALESCE(modified_by, created_by);");

    // Step 5: Now safe to drop old columns
    migrationBuilder.DropColumn(
        name: "archived_at",
        schema: "deeplynx",
        table: "classes");

    migrationBuilder.DropColumn(
        name: "created_by",
        schema: "deeplynx",
        table: "classes");

    migrationBuilder.DropColumn(
        name: "modified_at",
        schema: "deeplynx",
        table: "classes");
    
    migrationBuilder.RenameColumn(
        name: "modified_by",
        schema: "deeplynx",
        table: "classes",
        newName: "last_updated_by");

    migrationBuilder.RenameColumn(
        name: "created_at",
        schema: "deeplynx",
        table: "classes",
        newName: "last_updated_at");
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "classes",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "classes",
                newName: "created_at");

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "classes",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "classes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "classes",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}
