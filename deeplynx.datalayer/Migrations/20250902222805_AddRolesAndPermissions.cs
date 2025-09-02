using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_sysadmin",
                schema: "deeplynx",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    project_id = table.Column<long>(type: "bigint", nullable: false)
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
                name: "groups",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    organization_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("groups_pkey", x => x.id);
                    table.ForeignKey(
                        name: "groups_organization_id_fkey",
                        column: x => x.organization_id,
                        principalSchema: "deeplynx",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_users",
                schema: "deeplynx",
                columns: table => new
                {
                    organization_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    is_org_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_user_pkey", x => new { x.organization_id, x.user_id });
                    table.ForeignKey(
                        name: "organization_users_organization_id_fkey",
                        column: x => x.organization_id,
                        principalSchema: "deeplynx",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "organization_users_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sensitivity_labels",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    organization_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sensitivity_labels_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sensitivity_labels_organization_id_fkey",
                        column: x => x.organization_id,
                        principalSchema: "deeplynx",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_users",
                schema: "deeplynx",
                columns: table => new
                {
                    group_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("group_users_pkey", x => new { x.group_id, x.user_id });
                    table.ForeignKey(
                        name: "group_users_group_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "group_users_user_id_fkey",
                        column: x => x.group_id,
                        principalSchema: "deeplynx",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_members",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    group_id = table.Column<long>(type: "bigint", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("project_members_pkey", x => x.id);
                    table.ForeignKey(
                        name: "project_members_group_id_fkey",
                        column: x => x.group_id,
                        principalSchema: "deeplynx",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "project_members_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "project_members_role_id_fkey",
                        column: x => x.role_id,
                        principalSchema: "deeplynx",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "project_members_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    domain = table.Column<string>(type: "text", nullable: true),
                    label_id = table.Column<long>(type: "bigint", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("permission_pkey", x => x.id);
                    table.ForeignKey(
                        name: "permissions_label_id_fkey",
                        column: x => x.label_id,
                        principalSchema: "deeplynx",
                        principalTable: "sensitivity_labels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "record_labels",
                schema: "deeplynx",
                columns: table => new
                {
                    record_id = table.Column<long>(type: "bigint", nullable: false),
                    label_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("record_labels_pkey", x => new { x.record_id, x.label_id });
                    table.ForeignKey(
                        name: "record_labels_label_id_fkey",
                        column: x => x.label_id,
                        principalSchema: "deeplynx",
                        principalTable: "sensitivity_labels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "record_labels_record_id_fkey",
                        column: x => x.record_id,
                        principalSchema: "deeplynx",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "deeplynx",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    permission_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("role_permissions_pkey", x => new { x.role_id, x.permission_id });
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

            migrationBuilder.CreateIndex(
                name: "idx_projects_organization_id",
                schema: "deeplynx",
                table: "projects",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_group_users_group_id",
                schema: "deeplynx",
                table: "group_users",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "idx_group_users_user_id",
                schema: "deeplynx",
                table: "group_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_groups_id",
                schema: "deeplynx",
                table: "groups",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_groups_organization_id",
                schema: "deeplynx",
                table: "groups",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_organization_users_organization_id",
                schema: "deeplynx",
                table: "organization_users",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "idx_organization_users_user_id",
                schema: "deeplynx",
                table: "organization_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "unique_organization_user_ids",
                schema: "deeplynx",
                table: "organization_users",
                columns: new[] { "organization_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_organizations_id",
                schema: "deeplynx",
                table: "organizations",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_id",
                schema: "deeplynx",
                table: "permissions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_label_id",
                schema: "deeplynx",
                table: "permissions",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "idx_project_members_group_id",
                schema: "deeplynx",
                table: "project_members",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "idx_project_members_id",
                schema: "deeplynx",
                table: "project_members",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_project_members_project_id",
                schema: "deeplynx",
                table: "project_members",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_project_members_role_id",
                schema: "deeplynx",
                table: "project_members",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_project_members_user_id",
                schema: "deeplynx",
                table: "project_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "unique_project_member_ids",
                schema: "deeplynx",
                table: "project_members",
                columns: new[] { "project_id", "group_id", "role_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_record_labels_label_id",
                schema: "deeplynx",
                table: "record_labels",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_labels_record_id",
                schema: "deeplynx",
                table: "record_labels",
                column: "record_id");

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
                name: "idx_sensitivity_labels_id",
                schema: "deeplynx",
                table: "sensitivity_labels",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_sensitivity_labels_name",
                schema: "deeplynx",
                table: "sensitivity_labels",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_sensitivity_labels_organization_id",
                schema: "deeplynx",
                table: "sensitivity_labels",
                column: "organization_id");

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

            migrationBuilder.DropTable(
                name: "group_users",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "organization_users",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "project_members",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "record_labels",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "groups",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "sensitivity_labels",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "deeplynx");

            migrationBuilder.DropIndex(
                name: "idx_projects_organization_id",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_sysadmin",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects");
        }
    }
}
