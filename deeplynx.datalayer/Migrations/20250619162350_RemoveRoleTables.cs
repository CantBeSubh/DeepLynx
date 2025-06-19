using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "role_resources",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "user_projects",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "users",
                schema: "deeplynx");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("permissions_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pkey", x => x.id);
                    table.ForeignKey(
                        name: "roles_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    permission_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    action_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("role_permissions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "role_permissions_permission_id_fkey",
                        column: x => x.permission_id,
                        principalSchema: "deeplynx",
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_permissions_role_id_fkey",
                        column: x => x.role_id,
                        principalSchema: "deeplynx",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_resources",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_source_id = table.Column<long>(type: "bigint", nullable: true),
                    record_id = table.Column<long>(type: "bigint", nullable: true),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: true),
                    has_access = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("role_resources_pkey", x => x.id);
                    table.ForeignKey(
                        name: "role_resources_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_resources_record_id_fkey",
                        column: x => x.record_id,
                        principalSchema: "deeplynx",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_resources_role_id_fkey",
                        column: x => x.role_id,
                        principalSchema: "deeplynx",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_resources_tag_id_fkey",
                        column: x => x.tag_id,
                        principalSchema: "deeplynx",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_projects",
                schema: "deeplynx",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_projects_pkey", x => new { x.user_id, x.project_id });
                    table.ForeignKey(
                        name: "user_projects_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_projects_role_id_fkey",
                        column: x => x.role_id,
                        principalSchema: "deeplynx",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_projects_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_permissions_id",
                schema: "deeplynx",
                table: "permissions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_action_type",
                schema: "deeplynx",
                table: "role_permissions",
                column: "action_type");

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_permission_id",
                schema: "deeplynx",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_role_id",
                schema: "deeplynx",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_resources_data_source_id",
                schema: "deeplynx",
                table: "role_resources",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_resources_record_id",
                schema: "deeplynx",
                table: "role_resources",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_resources_role_id",
                schema: "deeplynx",
                table: "role_resources",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_resources_tag_id",
                schema: "deeplynx",
                table: "role_resources",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "idx_roles_id",
                schema: "deeplynx",
                table: "roles",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_roles_project_id",
                schema: "deeplynx",
                table: "roles",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_projects_project_id",
                schema: "deeplynx",
                table: "user_projects",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_projects_role_id",
                schema: "deeplynx",
                table: "user_projects",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_projects_user_id",
                schema: "deeplynx",
                table: "user_projects",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_id",
                schema: "deeplynx",
                table: "users",
                column: "id");
        }
    }
}
