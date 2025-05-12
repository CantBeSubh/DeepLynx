using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "deeplynx");

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("permissions_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    abbreviation = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("projects_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "classes",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    uuid = table.Column<string>(type: "text", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    abbreviation = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    base_uri = table.Column<string>(type: "text", nullable: true),
                    config = table.Column<string>(type: "jsonb", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                name: "roles",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                name: "tags",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                name: "relationships",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    uuid = table.Column<string>(type: "text", nullable: true),
                    origin_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                name: "records",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uri = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "jsonb", nullable: false),
                    original_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    custom_id = table.Column<string>(type: "text", nullable: true),
                    class_id = table.Column<long>(type: "bigint", nullable: true),
                    class_name = table.Column<string>(type: "text", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "records_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    permission_id = table.Column<long>(type: "bigint", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "record_mappings",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    record_params = table.Column<string>(type: "jsonb", nullable: false),
                    class_id = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    tag_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("record_mappings_pkey", x => x.id);
                    table.ForeignKey(
                        name: "record_mappings_class_id_fkey",
                        column: x => x.class_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "record_mappings_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "record_mappings_tag_id_fkey",
                        column: x => x.tag_id,
                        principalSchema: "deeplynx",
                        principalTable: "tags",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "edge_mappings",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    origin_params = table.Column<string>(type: "jsonb", nullable: false),
                    destination_params = table.Column<string>(type: "jsonb", nullable: false),
                    relationship_id = table.Column<long>(type: "bigint", nullable: false),
                    origin_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edge_mappings_pkey", x => x.id);
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
                name: "edges",
                schema: "deeplynx",
                columns: table => new
                {
                    origin_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_id = table.Column<long>(type: "bigint", nullable: false),
                    properties = table.Column<string>(type: "jsonb", nullable: true),
                    relationship_id = table.Column<long>(type: "bigint", nullable: true),
                    relationship_name = table.Column<string>(type: "text", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: true),
                    project_id = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edges_pkey", x => new { x.origin_id, x.destination_id });
                    table.ForeignKey(
                        name: "edges_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "edges_destination_id_fkey",
                        column: x => x.destination_id,
                        principalSchema: "deeplynx",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "role_resources",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    data_source_id = table.Column<long>(type: "bigint", nullable: true),
                    tag_id = table.Column<long>(type: "bigint", nullable: true),
                    record_id = table.Column<long>(type: "bigint", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "idx_classes_id",
                schema: "deeplynx",
                table: "classes",
                column: "id");

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
                name: "idx_permissions_id",
                schema: "deeplynx",
                table: "permissions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_projects_id",
                schema: "deeplynx",
                table: "projects",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_class_id",
                schema: "deeplynx",
                table: "record_mappings",
                column: "class_id");

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
                name: "idx_records_class_name",
                schema: "deeplynx",
                table: "records",
                column: "class_name");

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
                name: "idx_tags_id",
                schema: "deeplynx",
                table: "tags",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_tags_project_id",
                schema: "deeplynx",
                table: "tags",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "edge_mappings",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "edges",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "record_mappings",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "record_tags",
                schema: "deeplynx");

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
                name: "relationships",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "records",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "users",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "classes",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "data_sources",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "deeplynx");
        }
    }
}
