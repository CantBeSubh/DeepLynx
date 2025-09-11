using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "deeplynx");

            migrationBuilder.CreateTable(
                name: "logs",
                schema: "deeplynx",
                columns: table => new
                {
                    message = table.Column<string>(type: "text", nullable: true),
                    message_template = table.Column<string>(type: "text", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    exception = table.Column<string>(type: "text", nullable: true),
                    log_event = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                });

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
                name: "users",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_sys_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
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
                name: "projects",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    abbreviation = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    config = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{\"tagsMutable\": false, \"ontologyMutable\": false, \"edgeRecordsMutable\": false}'::jsonb"),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    organization_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("projects_pkey", x => x.id);
                    table.ForeignKey(
                        name: "projects_organization_id_fkey",
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
                name: "actions",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    config = table.Column<string>(type: "jsonb", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("actions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "actions_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "classes",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    uuid = table.Column<string>(type: "text", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("classes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "classes_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_sources",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    abbreviation = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    base_uri = table.Column<string>(type: "text", nullable: true),
                    config = table.Column<string>(type: "jsonb", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    @default = table.Column<bool>(name: "default", type: "boolean", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("data_sources_pkey", x => x.id);
                    table.ForeignKey(
                        name: "data_sources_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "object_storages",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    config = table.Column<string>(type: "jsonb", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    @default = table.Column<bool>(name: "default", type: "boolean", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("object_storage_pkey", x => x.id);
                    table.ForeignKey(
                        name: "object_storage_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
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
                name: "tags",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tags_pkey", x => x.id);
                    table.ForeignKey(
                        name: "tags_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_project",
                schema: "deeplynx",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_project_pkey", x => new { x.user_id, x.project_id });
                    table.ForeignKey(
                        name: "user_project_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_project_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "relationships",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    uuid = table.Column<string>(type: "text", nullable: true),
                    origin_id = table.Column<long>(type: "bigint", nullable: true),
                    destination_id = table.Column<long>(type: "bigint", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("relationships_pkey", x => x.id);
                    table.ForeignKey(
                        name: "relationships_destination_id_fkey",
                        column: x => x.destination_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "relationships_origin_id_fkey",
                        column: x => x.origin_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "relationships_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "events",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operation = table.Column<string>(type: "text", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    data_source_id = table.Column<long>(type: "bigint", nullable: true),
                    properties = table.Column<string>(type: "jsonb", nullable: false),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("events_pkey", x => x.id);
                    table.ForeignKey(
                        name: "events_dataSource_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "events_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "record_mappings",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    record_params = table.Column<string>(type: "jsonb", nullable: false),
                    class_id = table.Column<long>(type: "bigint", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("record_mappings_pkey", x => x.id);
                    table.ForeignKey(
                        name: "record_mapping_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "record_mappings_class_id_fkey",
                        column: x => x.class_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "record_mappings_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    action_id = table.Column<long>(type: "bigint", nullable: false),
                    operation = table.Column<string>(type: "text", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    data_source_id = table.Column<long>(type: "bigint", nullable: true),
                    entity_type = table.Column<string>(type: "text", nullable: true),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("subscriptions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "subscriptions_action_id_fkey",
                        column: x => x.action_id,
                        principalSchema: "deeplynx",
                        principalTable: "actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "subscriptions_dataSource_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "subscriptions_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "subscriptions_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
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
                name: "edge_mappings",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    origin_params = table.Column<string>(type: "jsonb", nullable: false),
                    destination_params = table.Column<string>(type: "jsonb", nullable: false),
                    relationship_id = table.Column<long>(type: "bigint", nullable: false),
                    origin_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edge_mappings_pkey", x => x.id);
                    table.ForeignKey(
                        name: "edge_mappings_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_mappings_destination_id_fkey",
                        column: x => x.destination_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_mappings_origin_id_fkey",
                        column: x => x.origin_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_mappings_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_mappings_relationship_id_fkey",
                        column: x => x.relationship_id,
                        principalSchema: "deeplynx",
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "record_mapping_tags",
                schema: "deeplynx",
                columns: table => new
                {
                    RecordMappingId = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: false),
                    record_mapping_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("record_mapping_tags_pkey", x => new { x.RecordMappingId, x.tag_id });
                    table.ForeignKey(
                        name: "record_mapping_tags_record_mapping_id_fkey",
                        column: x => x.RecordMappingId,
                        principalSchema: "deeplynx",
                        principalTable: "record_mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "record_mapping_tags_tag_id_fkey",
                        column: x => x.tag_id,
                        principalSchema: "deeplynx",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "records",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    uri = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "jsonb", nullable: false),
                    original_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    class_id = table.Column<long>(type: "bigint", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    mapping_id = table.Column<long>(type: "bigint", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    object_storage_id = table.Column<long>(type: "bigint", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("records_pkey", x => x.id);
                    table.ForeignKey(
                        name: "records_class_id_fkey",
                        column: x => x.class_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "records_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "records_mapping_id_fkey",
                        column: x => x.mapping_id,
                        principalSchema: "deeplynx",
                        principalTable: "record_mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "records_object_storage_id_fkey",
                        column: x => x.object_storage_id,
                        principalSchema: "deeplynx",
                        principalTable: "object_storages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "records_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
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

            migrationBuilder.CreateTable(
                name: "edges",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    origin_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_id = table.Column<long>(type: "bigint", nullable: false),
                    relationship_id = table.Column<long>(type: "bigint", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    mapping_id = table.Column<long>(type: "bigint", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edges_pkey", x => x.id);
                    table.ForeignKey(
                        name: "edges_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edges_destination_id_fkey",
                        column: x => x.destination_id,
                        principalSchema: "deeplynx",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edges_mapping_id_fkey",
                        column: x => x.mapping_id,
                        principalSchema: "deeplynx",
                        principalTable: "edge_mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "edges_origin_id_fkey",
                        column: x => x.origin_id,
                        principalSchema: "deeplynx",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edges_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edges_relationship_id_fkey",
                        column: x => x.relationship_id,
                        principalSchema: "deeplynx",
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "historical_records",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    record_id = table.Column<long>(type: "bigint", nullable: false),
                    uri = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "jsonb", nullable: false),
                    original_id = table.Column<string>(type: "text", nullable: true),
                    class_id = table.Column<long>(type: "bigint", nullable: true),
                    class_name = table.Column<string>(type: "text", nullable: true),
                    mapping_id = table.Column<long>(type: "bigint", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    data_source_name = table.Column<string>(type: "text", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    project_name = table.Column<string>(type: "text", nullable: false),
                    tags = table.Column<string>(type: "jsonb", nullable: true),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    description = table.Column<string>(type: "text", nullable: true),
                    object_storage_id = table.Column<long>(type: "bigint", nullable: true),
                    object_storage_name = table.Column<string>(type: "text", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("historical_records_pkey", x => x.id);
                    table.ForeignKey(
                        name: "historical_records_record_id_fkey",
                        column: x => x.record_id,
                        principalSchema: "deeplynx",
                        principalTable: "records",
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
                name: "record_tags",
                schema: "deeplynx",
                columns: table => new
                {
                    record_id = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("record_tags_pkey", x => new { x.record_id, x.tag_id });
                    table.ForeignKey(
                        name: "record_tags_record_id_fkey",
                        column: x => x.record_id,
                        principalSchema: "deeplynx",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "record_tags_tag_id_fkey",
                        column: x => x.tag_id,
                        principalSchema: "deeplynx",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "historical_edges",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    edge_id = table.Column<long>(type: "bigint", nullable: false),
                    origin_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_id = table.Column<long>(type: "bigint", nullable: false),
                    relationship_id = table.Column<long>(type: "bigint", nullable: true),
                    relationship_name = table.Column<string>(type: "text", nullable: true),
                    mapping_id = table.Column<long>(type: "bigint", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_source_name = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text"),
                    project_name = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text"),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("historical_edges_pkey", x => x.id);
                    table.ForeignKey(
                        name: "historical_edges_edge_id_fkey",
                        column: x => x.edge_id,
                        principalSchema: "deeplynx",
                        principalTable: "edges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_actions_id",
                schema: "deeplynx",
                table: "actions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_project_id",
                schema: "deeplynx",
                table: "actions",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_classes_id",
                schema: "deeplynx",
                table: "classes",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_classes_name",
                schema: "deeplynx",
                table: "classes",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_classes_project_id",
                schema: "deeplynx",
                table: "classes",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_classes_uuid",
                schema: "deeplynx",
                table: "classes",
                column: "uuid");

            migrationBuilder.CreateIndex(
                name: "unique_class_name",
                schema: "deeplynx",
                table: "classes",
                columns: new[] { "project_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_data_sources_id",
                schema: "deeplynx",
                table: "data_sources",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_data_sources_project_id",
                schema: "deeplynx",
                table: "data_sources",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_data_source_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_destination_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_origin_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_project_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_relationship_id",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_data_source_id",
                schema: "deeplynx",
                table: "edges",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_destination_id",
                schema: "deeplynx",
                table: "edges",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_id",
                schema: "deeplynx",
                table: "edges",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_mapping_id",
                schema: "deeplynx",
                table: "edges",
                column: "mapping_id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_origin_id",
                schema: "deeplynx",
                table: "edges",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_project_id",
                schema: "deeplynx",
                table: "edges",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_edges_relationship_id",
                schema: "deeplynx",
                table: "edges",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "unique_edge_record_ids",
                schema: "deeplynx",
                table: "edges",
                columns: new[] { "project_id", "origin_id", "destination_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_events_id",
                schema: "deeplynx",
                table: "events",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_events_project_id",
                schema: "deeplynx",
                table: "events",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_events_data_source_id",
                schema: "deeplynx",
                table: "events",
                column: "data_source_id");

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
                name: "idx_historical_edges_destination_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_edge_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "edge_id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_last_updated_at",
                schema: "deeplynx",
                table: "historical_edges",
                column: "last_updated_at");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_origin_id",
                schema: "deeplynx",
                table: "historical_edges",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_relationship_name",
                schema: "deeplynx",
                table: "historical_edges",
                column: "relationship_name");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_class_name",
                schema: "deeplynx",
                table: "historical_records",
                column: "class_name");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_id",
                schema: "deeplynx",
                table: "historical_records",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_last_updated_at",
                schema: "deeplynx",
                table: "historical_records",
                column: "last_updated_at");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_properties",
                schema: "deeplynx",
                table: "historical_records",
                column: "properties")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_record_id",
                schema: "deeplynx",
                table: "historical_records",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "idx_object_storage_id",
                schema: "deeplynx",
                table: "object_storages",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_object_storages_project_id",
                schema: "deeplynx",
                table: "object_storages",
                column: "project_id");

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
                name: "idx_projects_id",
                schema: "deeplynx",
                table: "projects",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_projects_organization_id",
                schema: "deeplynx",
                table: "projects",
                column: "organization_id");

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
                name: "idx_record_mapping_tags_record_mapping_id",
                schema: "deeplynx",
                table: "record_mapping_tags",
                column: "RecordMappingId");

            migrationBuilder.CreateIndex(
                name: "idx_record_mapping_tags_tag_id",
                schema: "deeplynx",
                table: "record_mapping_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_class_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_data_source_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_project_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_tag_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_tags_record_id",
                schema: "deeplynx",
                table: "record_tags",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_tags_tag_id",
                schema: "deeplynx",
                table: "record_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "idx_records_class_id",
                schema: "deeplynx",
                table: "records",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "idx_records_data_source_id",
                schema: "deeplynx",
                table: "records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_records_id",
                schema: "deeplynx",
                table: "records",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_records_mapping_id",
                schema: "deeplynx",
                table: "records",
                column: "mapping_id");

            migrationBuilder.CreateIndex(
                name: "idx_records_name",
                schema: "deeplynx",
                table: "records",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_records_original_id",
                schema: "deeplynx",
                table: "records",
                column: "original_id");

            migrationBuilder.CreateIndex(
                name: "idx_records_project_id",
                schema: "deeplynx",
                table: "records",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_records_properties",
                schema: "deeplynx",
                table: "records",
                column: "properties")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_records_object_storage_id",
                schema: "deeplynx",
                table: "records",
                column: "object_storage_id");

            migrationBuilder.CreateIndex(
                name: "unique_record_original_id",
                schema: "deeplynx",
                table: "records",
                columns: new[] { "project_id", "data_source_id", "original_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_relationships_destination_id",
                schema: "deeplynx",
                table: "relationships",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "idx_relationships_id",
                schema: "deeplynx",
                table: "relationships",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_relationships_name",
                schema: "deeplynx",
                table: "relationships",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_relationships_origin_id",
                schema: "deeplynx",
                table: "relationships",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "idx_relationships_project_id",
                schema: "deeplynx",
                table: "relationships",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_relationships_uuid",
                schema: "deeplynx",
                table: "relationships",
                column: "uuid");

            migrationBuilder.CreateIndex(
                name: "unique_relationship_name",
                schema: "deeplynx",
                table: "relationships",
                columns: new[] { "project_id", "name" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_entity_type",
                schema: "deeplynx",
                table: "subscriptions",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_project_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_user_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_unique_subscription",
                schema: "deeplynx",
                table: "subscriptions",
                columns: new[] { "user_id", "action_id", "operation", "project_id", "data_source_id", "entity_type", "entity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_action_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "action_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_data_source_id",
                schema: "deeplynx",
                table: "subscriptions",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "idx_tags_id",
                schema: "deeplynx",
                table: "tags",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_tags_name",
                schema: "deeplynx",
                table: "tags",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_tags_project_id",
                schema: "deeplynx",
                table: "tags",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "unique_tag_name",
                schema: "deeplynx",
                table: "tags",
                columns: new[] { "project_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_project_project_id",
                schema: "deeplynx",
                table: "user_project",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_project_user_id",
                schema: "deeplynx",
                table: "user_project",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                schema: "deeplynx",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_users_id",
                schema: "deeplynx",
                table: "users",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "group_users",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "historical_edges",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "historical_records",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "logs",
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
                name: "record_mapping_tags",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "record_tags",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "user_project",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "edges",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "groups",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "actions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "users",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "records",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "edge_mappings",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "sensitivity_labels",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "record_mappings",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "object_storages",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "relationships",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "data_sources",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "classes",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "deeplynx");
        }
    }
}
