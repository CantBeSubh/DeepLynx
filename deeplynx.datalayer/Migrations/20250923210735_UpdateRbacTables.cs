using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRbacTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.DropForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.RenameColumn(
                name: "domain",
                schema: "deeplynx",
                table: "permissions",
                newName: "resource");

            migrationBuilder.RenameIndex(
                name: "idx_permissions_domain",
                schema: "deeplynx",
                table: "permissions",
                newName: "idx_permissions_resource");

            migrationBuilder.AlterColumn<long>(
                name: "role_id",
                schema: "deeplynx",
                table: "project_members",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<bool>(
                name: "is_hardcoded",
                schema: "deeplynx",
                table: "permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "permissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "permissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "events",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "events",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_permissions_is_hardcoded",
                schema: "deeplynx",
                table: "permissions",
                column: "is_hardcoded");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_organization_id",
                schema: "deeplynx",
                table: "permissions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_project_id",
                schema: "deeplynx",
                table: "permissions",
                column: "project_id");

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

            migrationBuilder.AddForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "permissions_organization_id_fkey",
                schema: "deeplynx",
                table: "permissions",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "permissions_project_id_fkey",
                schema: "deeplynx",
                table: "permissions",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "role_id",
                principalSchema: "deeplynx",
                principalTable: "roles",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "deeplynx",
                table: "project_members",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_role(arc_role_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.roles 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE id = arc_role_id;
                    -- remove this role from anyone who holds it
                    UPDATE deeplynx.project_members 
                        SET role_id = NULL
                        WHERE role_id = arc_role_id;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_role;");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "deeplynx",
                table: "project_members",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.DropForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "permissions_organization_id_fkey",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropForeignKey(
                name: "permissions_project_id_fkey",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.DropForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "idx_permissions_is_hardcoded",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "idx_permissions_organization_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "idx_permissions_project_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_label_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "permissions_unique_resource_action",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "is_hardcoded",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "project_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.RenameColumn(
                name: "resource",
                schema: "deeplynx",
                table: "permissions",
                newName: "domain");

            migrationBuilder.RenameIndex(
                name: "idx_permissions_resource",
                schema: "deeplynx",
                table: "permissions",
                newName: "idx_permissions_domain");

            migrationBuilder.AlterColumn<long>(
                name: "role_id",
                schema: "deeplynx",
                table: "project_members",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "events",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "events_project_id_fkey",
                schema: "deeplynx",
                table: "events",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "role_id",
                principalSchema: "deeplynx",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
