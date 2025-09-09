using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixArchiveProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "subscriptions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "subscriptions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

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
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.projects SET is_archived = false, last_updated_at = archive_time WHERE id = arc_project_id;
                    UPDATE deeplynx.classes SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.data_sources SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.object_storages SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.record_mappings SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = false, last_updated_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                END;
                $BODY$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                    archive_time TIMESTAMP := NOW();
                BEGIN
                    UPDATE deeplynx.projects SET is_archived = false, last_updated_at = archive_time WHERE id = arc_project_id;
                    UPDATE deeplynx.classes SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.data_sources SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships SET is_archived = false, last_updated_at = archive_time WHERE project_id = arc_project_id;
                END;
                $BODY$;
            ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "subscriptions",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "subscriptions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "subscriptions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
