using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivalStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // add stored procedures for cascading archivals. specify deeplynx schema explicitly
            // archive project and children
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_project(arc_project_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.projects SET archived_at = arc_time WHERE id = arc_project_id;
                    UPDATE deeplynx.data_sources SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.classes SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edge_mappings SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.record_mappings SET archived_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE deeplynx.tags SET archived_at = arc_time WHERE project_id = arc_project_id;
                END;
                $$;
            ");

            // archive record and attached edges
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_record(arc_record_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.records SET archived_at = arc_time WHERE id = arc_record_id;
                    UPDATE deeplynx.edges SET archived_at = arc_time WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
                END;
                $$;
            ");

            // archive class and attached relationships and mappings
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_class(arc_class_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.classes SET archived_at = arc_time WHERE id = arc_class_id;
                    UPDATE deeplynx.relationships SET archived_at = arc_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.edge_mappings SET archived_at = arc_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.record_mappings SET archived_at = arc_time WHERE class_id = arc_class_id;
                END;
                $$;
            ");

            // archive relationship and attached mappings
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_relationship(arc_rel_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.relationships SET archived_at = arc_time WHERE id = arc_rel_id;
                    UPDATE deeplynx.edge_mappings SET archived_at = arc_time WHERE relationship_id = arc_rel_id;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_project;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_record;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_class;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_relationship;");
        }
    }
}
