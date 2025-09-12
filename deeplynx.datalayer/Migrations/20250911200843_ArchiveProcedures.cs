using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_project(arc_project_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.projects 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE id = arc_project_id;
                    UPDATE deeplynx.data_sources 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.classes 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.object_storages 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.tags 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                END;
                $$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_project(arc_project_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.projects 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE id = arc_project_id;
                    UPDATE deeplynx.data_sources 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.classes 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.object_storages 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                    UPDATE deeplynx.tags 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE project_id = arc_project_id;
                END;
                $$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_class(arc_class_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.classes 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE id = arc_class_id;
                    UPDATE deeplynx.relationships 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                END;
                $$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_class(arc_class_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.classes 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE id = arc_class_id;
                    UPDATE deeplynx.relationships 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                END;
                $$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_record(arc_record_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.records 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE id = arc_record_id;
                    UPDATE deeplynx.edges 
                        SET is_archived = TRUE, last_updated_at = arc_time 
                        WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
                END;
                $$;
            ");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_record(arc_record_id INTEGER, arc_time TIMESTAMP WITHOUT TIME ZONE)
                LANGUAGE plpgsql AS $$
                BEGIN
                    UPDATE deeplynx.records 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE id = arc_record_id;
                    UPDATE deeplynx.edges 
                        SET is_archived = FALSE, last_updated_at = arc_time 
                        WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_project;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_class;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_record;");

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_project;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_class;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_record;");
        }
    }
}
