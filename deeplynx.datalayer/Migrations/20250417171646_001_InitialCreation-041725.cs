using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class _001_InitialCreation041725 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "deeplynx");

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
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                name: "user_projects",
                schema: "deeplynx",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false)
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
                        name: "user_projects_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "record_parameters",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    record_params = table.Column<string>(type: "jsonb", nullable: false),
                    class_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("record_parameters_pkey", x => x.id);
                    table.ForeignKey(
                        name: "record_parameters_class_id_fkey",
                        column: x => x.class_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
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
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                name: "user_roles",
                schema: "deeplynx",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_roles_pkey", x => new { x.role_id, x.user_id });
                    table.ForeignKey(
                        name: "user_roles_role_id_fkey",
                        column: x => x.role_id,
                        principalSchema: "deeplynx",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_roles_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "deeplynx",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "edge_parameters",
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
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edge_parameters_pkey", x => x.id);
                    table.ForeignKey(
                        name: "edge_parameters_destination_id_fkey",
                        column: x => x.destination_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_parameters_origin_id_fkey",
                        column: x => x.origin_id,
                        principalSchema: "deeplynx",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_parameters_project_id_fkey",
                        column: x => x.project_id,
                        principalSchema: "deeplynx",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "edge_parameters_relationship_id_fkey",
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
                    relationship_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edges_pkey", x => new { x.origin_id, x.destination_id });
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
                        name: "edges_relationship_id_fkey",
                        column: x => x.relationship_id,
                        principalSchema: "deeplynx",
                        principalTable: "relationships",
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
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    data_source_id = table.Column<long>(type: "bigint", nullable: true),
                    record_id = table.Column<long>(type: "bigint", nullable: true),
                    access_type = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("permissions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "permissions_data_source_id_fkey",
                        column: x => x.data_source_id,
                        principalSchema: "deeplynx",
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "permissions_record_id_fkey",
                        column: x => x.record_id,
                        principalSchema: "deeplynx",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "permissions_role_id_fkey",
                        column: x => x.role_id,
                        principalSchema: "deeplynx",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_classes_project_id",
                schema: "deeplynx",
                table: "classes",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_sources_project_id",
                schema: "deeplynx",
                table: "data_sources",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_edge_parameters_destination_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "IX_edge_parameters_origin_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "IX_edge_parameters_project_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_edge_parameters_relationship_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_edges_destination_id",
                schema: "deeplynx",
                table: "edges",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "IX_edges_relationship_id",
                schema: "deeplynx",
                table: "edges",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_data_source_id",
                schema: "deeplynx",
                table: "permissions",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_record_id",
                schema: "deeplynx",
                table: "permissions",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_role_id",
                schema: "deeplynx",
                table: "permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_parameters_class_id",
                schema: "deeplynx",
                table: "record_parameters",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_records_class_id",
                schema: "deeplynx",
                table: "records",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_records_data_source_id",
                schema: "deeplynx",
                table: "records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "IX_records_project_id",
                schema: "deeplynx",
                table: "records",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_destination_id",
                schema: "deeplynx",
                table: "relationships",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_origin_id",
                schema: "deeplynx",
                table: "relationships",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationships_project_id",
                schema: "deeplynx",
                table: "relationships",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_project_id",
                schema: "deeplynx",
                table: "roles",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_projects_project_id",
                schema: "deeplynx",
                table: "user_projects",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id",
                schema: "deeplynx",
                table: "user_roles",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "edge_parameters",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "edges",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "record_parameters",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "user_projects",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "relationships",
                schema: "deeplynx");

            migrationBuilder.DropTable(
                name: "records",
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
