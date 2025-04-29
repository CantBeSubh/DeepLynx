using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "permissions_data_source_id_fkey",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropForeignKey(
                name: "permissions_record_id_fkey",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropForeignKey(
                name: "permissions_role_id_fkey",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "deeplynx");

            migrationBuilder.DropIndex(
                name: "IX_permissions_data_source_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "IX_permissions_record_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "IX_permissions_role_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "access_type",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "data_source_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "record_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "role_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<long>(
                name: "role_id",
                schema: "deeplynx",
                table: "user_projects",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "relationships",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "records",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "record_parameters",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<long>(
                name: "project_id",
                schema: "deeplynx",
                table: "record_parameters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "projects",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "permissions",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "deeplynx",
                table: "permissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "edge_parameters",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "data_sources",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "classes",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

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
                name: "idx_users_id",
                schema: "deeplynx",
                table: "users",
                column: "id");

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
                name: "idx_record_parameters_class_id",
                schema: "deeplynx",
                table: "record_parameters",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "idx_record_parameters_id",
                schema: "deeplynx",
                table: "record_parameters",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_record_parameters_project_id",
                schema: "deeplynx",
                table: "record_parameters",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_projects_id",
                schema: "deeplynx",
                table: "projects",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_id",
                schema: "deeplynx",
                table: "permissions",
                column: "id");

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
                name: "idx_edges_relationship_id",
                schema: "deeplynx",
                table: "edges",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_parameters_destination_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_parameters_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_parameters_origin_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "origin_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_parameters_project_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_edge_parameters_relationship_id",
                schema: "deeplynx",
                table: "edge_parameters",
                column: "relationship_id");

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
                name: "idx_tags_id",
                schema: "deeplynx",
                table: "tags",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_tags_project_id",
                schema: "deeplynx",
                table: "tags",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "record_parameters_project_id_fkey",
                schema: "deeplynx",
                table: "record_parameters",
                column: "project_id",
                principalSchema: "deeplynx",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "user_projects_role_id_fkey",
                schema: "deeplynx",
                table: "user_projects",
                column: "role_id",
                principalSchema: "deeplynx",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "record_parameters_project_id_fkey",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropForeignKey(
                name: "user_projects_role_id_fkey",
                schema: "deeplynx",
                table: "user_projects");

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
                name: "tags",
                schema: "deeplynx");

            migrationBuilder.DropIndex(
                name: "idx_users_id",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.DropIndex(
                name: "idx_user_projects_project_id",
                schema: "deeplynx",
                table: "user_projects");

            migrationBuilder.DropIndex(
                name: "idx_user_projects_role_id",
                schema: "deeplynx",
                table: "user_projects");

            migrationBuilder.DropIndex(
                name: "idx_user_projects_user_id",
                schema: "deeplynx",
                table: "user_projects");

            migrationBuilder.DropIndex(
                name: "idx_roles_id",
                schema: "deeplynx",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "idx_roles_project_id",
                schema: "deeplynx",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "idx_relationships_destination_id",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "idx_relationships_id",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "idx_relationships_origin_id",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "idx_relationships_project_id",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "idx_relationships_uuid",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropIndex(
                name: "idx_records_class_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_records_class_name",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_records_data_source_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_records_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_records_name",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_records_original_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_records_project_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_records_properties",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropIndex(
                name: "idx_record_parameters_class_id",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropIndex(
                name: "idx_record_parameters_id",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropIndex(
                name: "idx_record_parameters_project_id",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropIndex(
                name: "idx_projects_id",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "idx_permissions_id",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "idx_edges_destination_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "idx_edges_origin_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "idx_edges_relationship_id",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "idx_edge_parameters_destination_id",
                schema: "deeplynx",
                table: "edge_parameters");

            migrationBuilder.DropIndex(
                name: "idx_edge_parameters_id",
                schema: "deeplynx",
                table: "edge_parameters");

            migrationBuilder.DropIndex(
                name: "idx_edge_parameters_origin_id",
                schema: "deeplynx",
                table: "edge_parameters");

            migrationBuilder.DropIndex(
                name: "idx_edge_parameters_project_id",
                schema: "deeplynx",
                table: "edge_parameters");

            migrationBuilder.DropIndex(
                name: "idx_edge_parameters_relationship_id",
                schema: "deeplynx",
                table: "edge_parameters");

            migrationBuilder.DropIndex(
                name: "idx_data_sources_id",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropIndex(
                name: "idx_data_sources_project_id",
                schema: "deeplynx",
                table: "data_sources");

            migrationBuilder.DropIndex(
                name: "idx_classes_id",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "idx_classes_project_id",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "idx_classes_uuid",
                schema: "deeplynx",
                table: "classes");

            migrationBuilder.DropColumn(
                name: "role_id",
                schema: "deeplynx",
                table: "user_projects");

            migrationBuilder.DropColumn(
                name: "project_id",
                schema: "deeplynx",
                table: "record_parameters");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "deeplynx",
                table: "permissions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "relationships",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "records",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "record_parameters",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "projects",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "permissions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "access_type",
                schema: "deeplynx",
                table: "permissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "data_source_id",
                schema: "deeplynx",
                table: "permissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "record_id",
                schema: "deeplynx",
                table: "permissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "role_id",
                schema: "deeplynx",
                table: "permissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "edge_parameters",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "data_sources",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                schema: "deeplynx",
                table: "classes",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

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
                name: "IX_user_roles_user_id",
                schema: "deeplynx",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "permissions_data_source_id_fkey",
                schema: "deeplynx",
                table: "permissions",
                column: "data_source_id",
                principalSchema: "deeplynx",
                principalTable: "data_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "permissions_record_id_fkey",
                schema: "deeplynx",
                table: "permissions",
                column: "record_id",
                principalSchema: "deeplynx",
                principalTable: "records",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "permissions_role_id_fkey",
                schema: "deeplynx",
                table: "permissions",
                column: "role_id",
                principalSchema: "deeplynx",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
