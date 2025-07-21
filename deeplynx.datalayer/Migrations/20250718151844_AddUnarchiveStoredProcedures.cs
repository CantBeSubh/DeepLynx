using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AddUnarchiveStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // add stored procedures for cascading unarchive. specify deeplynx schema explicitly
            // unarchive project and children
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_project(arc_project_id INTEGER)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.projects SET archived_at = NULL WHERE id = arc_project_id;
                    UPDATE deeplynx.data_sources SET archived_at = NULL WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records SET archived_at = NULL WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges SET archived_at = NULL WHERE project_id = arc_project_id;
                    UPDATE deeplynx.classes SET archived_at = NULL WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships SET archived_at = NULL WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edge_mappings SET archived_at = NULL WHERE project_id = arc_project_id;
                    UPDATE deeplynx.record_mappings SET archived_at = NULL WHERE project_id = arc_project_id;
                    UPDATE deeplynx.tags SET archived_at = NULL WHERE project_id = arc_project_id;
                END;
                $$;
            ");

            // unarchive record and attached edges
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_record(arc_record_id INTEGER)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.records SET archived_at = NULL WHERE id = arc_record_id;
                    UPDATE deeplynx.edges SET archived_at = NULL WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
                END;
                $$;
            ");

            // unarchive class and attached relationships and mappings
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_class(arc_class_id INTEGER)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.classes SET archived_at = NULL WHERE id = arc_class_id;
                    UPDATE deeplynx.relationships SET archived_at = NULL WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.edge_mappings SET archived_at = NULL WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.record_mappings SET archived_at = NULL WHERE class_id = arc_class_id;
                END;
                $$;
            ");

            // unarchive relationship and attached mappings
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_relationship(arc_rel_id INTEGER)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.relationships SET archived_at = NULL WHERE id = arc_rel_id;
                    UPDATE deeplynx.edge_mappings SET archived_at = NULL WHERE relationship_id = arc_rel_id;
                END;
                $$;
            ");
            
            // unarchive data source and attached edge and record mappings
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_data_source(arc_data_source_id INTEGER)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.data_sources SET archived_at = NULL WHERE id = arc_data_source_id;
                    UPDATE deeplynx.record_mappings SET archived_at = NULL WHERE data_source_id = arc_data_source_id;
                    UPDATE deeplynx.edge_mappings SET archived_at = NULL WHERE data_source_id = arc_data_source_id;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_project;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_record;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_class;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_relationship;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_data_source;");
        }
    }
}
