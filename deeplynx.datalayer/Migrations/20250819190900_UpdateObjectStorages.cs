using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateObjectStorages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "object_storage_id",
                schema: "deeplynx",
                table: "records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "object_storage_id",
                schema: "deeplynx",
                table: "historical_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "object_storage_name",
                schema: "deeplynx",
                table: "historical_records",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "object_storage",
                schema: "deeplynx",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    config = table.Column<string>(type: "jsonb", nullable: false),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "idx_records_object_storage_id",
                schema: "deeplynx",
                table: "records",
                column: "object_storage_id");

            migrationBuilder.CreateIndex(
                name: "idx_object_storage_id",
                schema: "deeplynx",
                table: "object_storage",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_object_storage_project_id",
                schema: "deeplynx",
                table: "object_storage",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "records_object_storage_id_fkey",
                schema: "deeplynx",
                table: "records",
                column: "object_storage_id",
                principalSchema: "deeplynx",
                principalTable: "object_storage",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.RenameTable(
                name: "object_storage",
                schema: "deeplynx",
                newName: "object_storages",
                newSchema: "deeplynx");

            migrationBuilder.AddColumn<bool>(
                name: "default",
                schema: "deeplynx",
                table: "object_storages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "records_object_storage_id_fkey",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropTable(
                name: "object_storages",
                schema: "deeplynx");

            migrationBuilder.DropIndex(
                name: "idx_records_object_storage_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "object_storage_id",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "object_storage_id",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropColumn(
                name: "object_storage_name",
                schema: "deeplynx",
                table: "historical_records");
            
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_project;");
        }
    }
}
