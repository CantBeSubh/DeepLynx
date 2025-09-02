using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class CreatePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "project_members_group_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.DropForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.DropForeignKey(
                name: "project_members_user_id_fkey",
                schema: "deeplynx",
                table: "project_members");

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
                name: "project_members_group_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "group_id",
                principalSchema: "deeplynx",
                principalTable: "groups",
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
                name: "project_members_user_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "user_id",
                principalSchema: "deeplynx",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "project_members_group_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.DropForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.DropForeignKey(
                name: "project_members_user_id_fkey",
                schema: "deeplynx",
                table: "project_members");

            migrationBuilder.DropTable(
                name: "group_users",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "record_labels",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "sensitivity_labels",
                schema: "deeplynx");

            migrationBuilder.AddForeignKey(
                name: "project_members_group_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "project_members_role_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "project_members_user_id_fkey",
                schema: "deeplynx",
                table: "project_members",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
