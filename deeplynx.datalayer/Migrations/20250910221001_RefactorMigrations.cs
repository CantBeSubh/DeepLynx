using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE deeplynx.users DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.tags DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.tags DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.tags DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions DROP COLUMN IF EXISTS created_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.records DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.records DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.records DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS created_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS created_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events DROP COLUMN IF EXISTS created_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edges DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edges DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edges DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.classes DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.classes DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.classes DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions DROP COLUMN IF EXISTS created_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "tags",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "tags",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "relationships",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "relationships",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "records",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "records",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "projects",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "projects",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "object_storages",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "object_storages",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "historical_records",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "historical_edges",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "deeplynx",
                table: "events",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "edges",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "edges",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "last_updated_at");

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

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "actions",
                newName: "last_updated_by");

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_sys_admin",
                schema: "deeplynx",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "tags",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                schema: "deeplynx",
                table: "subscriptions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "subscriptions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "relationships",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "record_mappings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "object_storages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_records",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "historical_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_edges",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "historical_edges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "events",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "edges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "edge_mappings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "data_sources",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "classes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "actions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "actions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

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
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_pkey", x => x.id);
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
                    organization_id = table.Column<long>(type: "bigint", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
                    project_id = table.Column<long>(type: "bigint", nullable: true),
                    organization_id = table.Column<long>(type: "bigint", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pkey", x => x.id);
                    table.ForeignKey(
                        name: "roles_organization_id_fkey",
                        column: x => x.organization_id,
                        principalSchema: "deeplynx",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "roles_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
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
                    project_id = table.Column<long>(type: "bigint", nullable: true),
                    organization_id = table.Column<long>(type: "bigint", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sensitivity_labels_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sensitivity_label_organization_id_fkey",
                        column: x => x.organization_id,
                        principalSchema: "deeplynx",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "sensitivity_label_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
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
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
                name: "idx_unique_subscription",
                schema: "deeplynx",
                table: "subscriptions",
                columns: new[] { "user_id", "action_id", "operation", "project_id", "data_source_id", "entity_type", "entity_id" },
                unique: true);

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
                name: "idx_permissions_action",
                schema: "deeplynx",
                table: "permissions",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_domain",
                schema: "deeplynx",
                table: "permissions",
                column: "domain");

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
                name: "idx_roles_organization_id",
                schema: "deeplynx",
                table: "roles",
                column: "organization_id");

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

            migrationBuilder.CreateIndex(
                name: "idx_sensitivity_labels_project_id",
                schema: "deeplynx",
                table: "sensitivity_labels",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "projects_organization_id_fkey",
                schema: "deeplynx",
                table: "projects",
                column: "organization_id",
                principalSchema: "deeplynx",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            // Stored Procedures
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_class(IN arc_class_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                DECLARE
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.classes SET is_archived = true, last_updated_at = archive_time WHERE id = arc_class_id;
                    UPDATE deeplynx.relationships SET is_archived = true, last_updated_at = archive_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = true, last_updated_at = archive_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.record_mappings SET is_archived = true, last_updated_at = archive_time WHERE class_id = arc_class_id;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_class(IN arc_class_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                DECLARE
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.classes SET is_archived = false, last_updated_at = archive_time WHERE id = arc_class_id;
                    UPDATE deeplynx.relationships SET is_archived = false, last_updated_at = archive_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = false, last_updated_at = archive_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.record_mappings SET is_archived = false, last_updated_at = archive_time WHERE class_id = arc_class_id;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_data_source(IN arc_data_source_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                DECLARE
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.data_sources SET is_archived = true, last_updated_at = archive_time WHERE id = arc_data_source_id;
                    UPDATE deeplynx.record_mappings SET is_archived = true, last_updated_at = archive_time WHERE data_source_id = arc_data_source_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = true, last_updated_at = archive_time WHERE data_source_id = arc_data_source_id;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_data_source(IN arc_data_source_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                DECLARE
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.data_sources SET is_archived = false, last_updated_at = archive_time WHERE id = arc_data_source_id;
                    UPDATE deeplynx.record_mappings SET is_archived = false, last_updated_at = archive_time WHERE data_source_id = arc_data_source_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = false, last_updated_at = archive_time WHERE data_source_id = arc_data_source_id;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_project(
                    IN arc_project_id integer,
                    IN arc_time timestamp without time zone)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.projects SET is_archived = true, last_updated_at = arc_time WHERE id = arc_project_id;
                    UPDATE deeplynx.classes SET is_archived = true, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.data_sources SET is_archived = true, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.object_storages SET is_archived = true, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.record_mappings SET is_archived = true, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = true, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records SET is_archived = true, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges SET is_archived = true, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships SET is_archived = true, last_updated_at = arc_time WHERE project_id = arc_project_id;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_project(IN arc_project_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                DECLARE
                    arc_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.projects SET is_archived = false, last_updated_at = arc_time WHERE id = arc_project_id;
                    UPDATE deeplynx.classes SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.data_sources SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.object_storages SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.record_mappings SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_record(IN arc_record_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                DECLARE
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.records SET is_archived = true, last_updated_at = archive_time WHERE id = arc_record_id;
                    UPDATE deeplynx.edges SET is_archived = true, last_updated_at = archive_time WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_record(IN arc_record_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                DECLARE
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.records SET is_archived = false, last_updated_at = archive_time WHERE id = arc_record_id;
                    UPDATE deeplynx.edges SET is_archived = false, last_updated_at = archive_time WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_relationship(IN arc_rel_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                DECLARE
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.relationships SET is_archived = true, last_updated_at = archive_time WHERE id = arc_rel_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = true, last_updated_at = archive_time WHERE relationship_id = arc_rel_id;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_relationship(IN arc_rel_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                DECLARE
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.relationships SET is_archived = false, last_updated_at = archive_time WHERE id = arc_rel_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = false, last_updated_at = archive_time WHERE relationship_id = arc_rel_id;
                END;
                $BODY$;
            ");

            // Triggers
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_modified_at()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    NEW.last_updated_at = CURRENT_TIMESTAMP;
                    RETURN NEW;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.create_historical_edges_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        last_updated_at, last_updated_by, is_archived,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.last_updated_at, NEW.last_updated_by, NEW.is_archived,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_historical_edges_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        last_updated_at, last_updated_by, is_archived,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.last_updated_at, NEW.last_updated_by, NEW.is_archived,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.archive_historical_edges_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        last_updated_at, last_updated_by, is_archived,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.last_updated_at, NEW.last_updated_by, NEW.is_archived,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.unarchive_historical_edges_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        last_updated_at, last_updated_by, is_archived,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.last_updated_at, NEW.last_updated_by, NEW.is_archived,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.create_historical_records_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        last_updated_at, last_updated_by, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.last_updated_at, NEW.last_updated_by, NEW.is_archived,
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.last_updated_at, r.last_updated_by, r.is_archived, 
                            c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_historical_records_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        last_updated_at, last_updated_by, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.last_updated_at, NEW.last_updated_by, NEW.is_archived,
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.last_updated_at, r.last_updated_by, r.is_archived,
                            c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.archive_historical_records_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        last_updated_at, last_updated_by, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.last_updated_at, NEW.last_updated_by, NEW.is_archived,
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.last_updated_at, r.last_updated_by, r.is_archived,
                            c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.unarchive_historical_records_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        last_updated_at, last_updated_by, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.last_updated_at, NEW.last_updated_by, NEW.is_archived,
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.last_updated_at, r.last_updated_by, r.is_archived,
                            c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $BODY$;
            ");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.insert_recordtag_historical_record_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN  
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        last_updated_at, last_updated_by, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                        r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id, 
                        LOCALTIMESTAMP, r.last_updated_by, r.is_archived,
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.record_id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                        r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                        r.last_updated_at, r.last_updated_by, r.is_archived,
                        c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $BODY$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.delete_recordtag_historical_record_trigger()
                RETURNS trigger
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN  
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        last_updated_at, last_updated_by, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        OLD.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                        r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                        LOCALTIMESTAMP, r.last_updated_by, r.is_archived,
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = OLD.record_id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                        r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                        r.last_updated_at, r.last_updated_by, r.is_archived,
                        c.name, d.name, p.name, o.name;
                    RETURN OLD;
                END;
                $BODY$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback Trigger functions

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_modified_at()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.modified_at = CURRENT_TIMESTAMP;
                    RETURN NEW;
                END;
                $$ language 'plpgsql';
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.create_historical_edges_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        created_at, created_by, last_updated_at,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.created_at, NEW.created_by, 
                        NEW.created_at AS last_updated_at,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_historical_edges_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        created_at, created_by, modified_at, modified_by,
                        last_updated_at,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.created_at, NEW.created_by, NEW.modified_at, NEW.modified_by, 
                        NEW.modified_at AS last_updated_at,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.archive_historical_edges_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        created_at, created_by, modified_at, modified_by,
                        archived_at, last_updated_at,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.created_at, NEW.created_by, NEW.modified_at, NEW.modified_by, 
                        NEW.archived_at, NEW.archived_at AS last_updated_at,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.unarchive_historical_edges_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        created_at, created_by, modified_at, modified_by,
                        archived_at, last_updated_at,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.created_at, NEW.created_by, NEW.modified_at, NEW.modified_by, 
                        NEW.archived_at, NEW.modified_at AS last_updated_at,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.create_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, object_storage_id,
                   created_by, created_at,
                   last_updated_at, tags,
                   class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.created_by, NEW.created_at,
                        NEW.created_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        last_updated_at, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, New.object_storage_id,
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.modified_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.archive_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        archived_at, last_updated_at, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, New.object_storage_id,
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.archived_at, NEW.archived_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.unarchive_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        archived_at, last_updated_at, tags,
                        class_name, data_source_name, project_name,  object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.archived_at, NEW.modified_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.insert_recordtag_historical_record_trigger()
                RETURNS trigger AS $$
                BEGIN  
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        last_updated_at, tags,
                        class_name, data_source_name, project_name,  object_storage_name)
                    SELECT 
                        NEW.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                        r.class_id, r.mapping_id, r.data_source_id, r.project_id,  r.object_storage_id, 
                        r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
                        LOCALTIMESTAMP AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.record_id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                        r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                        r.created_by, r.created_at, r.modified_by, r.modified_at, 
                        r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.delete_recordtag_historical_record_trigger()
                RETURNS trigger AS $$
                BEGIN  
                -- Insert the new historical record
                INSERT INTO deeplynx.historical_records (
                    record_id, uri, name, description, properties, original_id, 
                    class_id, mapping_id, data_source_id, project_id, object_storage_id,
                    created_by, created_at, modified_by, modified_at, 
                    last_updated_at, tags,
                    class_name, data_source_name, project_name, object_storage_name)
                SELECT 
                    OLD.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                    r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                    r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
                    LOCALTIMESTAMP AS last_updated_at, 
                    json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                    c.name, d.name, p.name, o.name
                FROM deeplynx.records r
                LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                JOIN deeplynx.projects p ON p.id = r.project_id
                WHERE r.id = OLD.record_id
                GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                        r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                        r.created_by, r.created_at, r.modified_by, r.modified_at, 
                        r.archived_at, c.name, d.name, p.name,  o.name;
                RETURN OLD;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Rollback stored procs

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_project(arc_project_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.projects SET archived_at = arc_time WHERE id = arc_project_id;
                    UPDATE deeplynx.data_sources SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.classes SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.object_storages SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edge_mappings SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.record_mappings SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.tags SET archived_at = arc_time WHERE project_id = arc_project_id;
                END;
                $$;
            ");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.create_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, object_storage_id,
                   created_by, created_at,
                   last_updated_at, tags,
                   class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.created_by, NEW.created_at,
                        NEW.created_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.insert_recordtag_historical_record_trigger()
                 RETURNS trigger AS $$
             BEGIN  
                -- Insert the new historical record
                INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, object_storage_id,
                   created_by, created_at, modified_by, modified_at, 
                   last_updated_at, tags,
                   class_name, data_source_name, project_name,  object_storage_name)
                SELECT 
                   NEW.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                   r.class_id, r.mapping_id, r.data_source_id, r.project_id,  r.object_storage_id, 
                   r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
                   LOCALTIMESTAMP AS last_updated_at, 
                   json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                   c.name, d.name, p.name, o.name
                FROM deeplynx.records r
                LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                JOIN deeplynx.projects p ON p.id = r.project_id
                WHERE r.id = NEW.record_id
                GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                      r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                      r.created_by, r.created_at, r.modified_by, r.modified_at, 
                      r.archived_at, c.name, d.name, p.name, o.name;
                RETURN NEW;
             END;
             $$ LANGUAGE plpgsql;
            ");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.delete_recordtag_historical_record_trigger()
                 RETURNS trigger AS $$
             BEGIN  
                -- Insert the new historical record
                INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, object_storage_id,
                   created_by, created_at, modified_by, modified_at, 
                   last_updated_at, tags,
                   class_name, data_source_name, project_name, object_storage_name)
                SELECT 
                   OLD.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                   r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                   r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
                   LOCALTIMESTAMP AS last_updated_at, 
                   json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                   c.name, d.name, p.name, o.name
                FROM deeplynx.records r
                LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                JOIN deeplynx.projects p ON p.id = r.project_id
                WHERE r.id = OLD.record_id
                GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                      r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                      r.created_by, r.created_at, r.modified_by, r.modified_at, 
                      r.archived_at, c.name, d.name, p.name,  o.name;
                RETURN OLD;
             END;
             $$ LANGUAGE plpgsql;
            ");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        last_updated_at, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, New.object_storage_id,
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.modified_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.archive_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        archived_at, last_updated_at, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, New.object_storage_id,
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.archived_at, NEW.archived_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.unarchive_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        archived_at, last_updated_at, tags,
                        class_name, data_source_name, project_name,  object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.archived_at, NEW.modified_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

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
                name: "idx_unique_subscription",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "idx_projects_organization_id",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_sys_admin",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "events");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.DropColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "tags",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "tags",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "relationships",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "relationships",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "records",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "records",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "projects",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "projects",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "object_storages",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "object_storages",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "historical_records",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "historical_edges",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "events",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "edges",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "edges",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "created_at");

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

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "actions",
                newName: "modified_by");

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "tags",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "tags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "tags",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                schema: "deeplynx",
                table: "subscriptions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "subscriptions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "subscriptions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "subscriptions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "relationships",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "relationships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "relationships",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "records",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "records",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "records",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "record_mappings",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "record_mappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "record_mappings",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "projects",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "projects",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "object_storages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "object_storages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "object_storages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_records",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "historical_records",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "historical_records",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "historical_records",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "historical_records",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "historical_edges",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "historical_edges",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "historical_edges",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "historical_edges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "historical_edges",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "events",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "edges",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "edges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "edges",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "edge_mappings",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "edge_mappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "edge_mappings",
                type: "timestamp without time zone",
                nullable: true);

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

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                schema: "deeplynx",
                table: "actions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "deeplynx",
                table: "actions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "deeplynx",
                table: "actions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "actions",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}