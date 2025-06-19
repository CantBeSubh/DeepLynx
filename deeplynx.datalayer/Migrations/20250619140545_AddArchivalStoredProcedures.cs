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
            // add stored procedures for cascading archivals
            // archive project and children
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE archive_project(arc_project_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE projects SET deleted_at = arc_time WHERE id = arc_project_id;
                    UPDATE roles SET deleted_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE data_sources SET deleted_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE records SET deleted_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE edges SET deleted_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE classes SET deleted_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE relationships SET deleted_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE edge_mappings SET deleted_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE record_mappings SET deleted_at = arc_time WHERE project_id = arc_project_id;
                    UPDATE tags SET deleted_at = arc_time WHERE project_id = arc_project_id;
                END;
                $$;
            ");

            // archive record and attached edges
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE archive_record(arc_record_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE records SET deleted_at = arc_time WHERE id = arc_record_id;
                    UPDATE edges SET deleted_at = arc_time WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
                END;
                $$;
            ");

            // archive class and attached relationships and mappings
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE archive_class(arc_class_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE classes SET deleted_at = arc_time WHERE id = arc_class_id;
                    UPDATE relationships SET deleted_at = arc_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE edge_mappings SET deleted_at = arc_time WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE record_mappings SET deleted_at = arc_time WHERE class_id = arc_class_id;
                END;
                $$;
            ");

            // archive relationship and attached mappings
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE archive_relationship(arc_rel_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE relationships SET deleted_at = arc_time WHERE id = arc_rel_id;
                    UPDATE edge_mappings SET deleted_at = arc_time WHERE relationship_id = arc_rel_id;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS archive_project;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS archive_record;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS archive_class;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS archive_relationship;");
        }
    }
}
