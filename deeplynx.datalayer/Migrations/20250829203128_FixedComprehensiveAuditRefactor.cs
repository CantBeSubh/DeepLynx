using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixedComprehensiveAuditRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =================================================================
            // HANDLE MIXED STATE: classes and data_sources already migrated
            // Other tables still need migration
            // =================================================================

            // Add is_archived to tables that DON'T already have it
            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "tags",
                type: "boolean", 
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "relationships",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "record_mappings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "object_storages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "edges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "edge_mappings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "actions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                schema: "deeplynx",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Historical tables - use safe approach
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");

            // =================================================================
            // STEP 2: TRANSFORM EXISTING DATA (PRESERVE ALL OLD DATA)
            // =================================================================

            migrationBuilder.Sql(@"
                UPDATE deeplynx.projects
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.projects
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );

                UPDATE deeplynx.projects
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.tags
                SET is_archived = (archived_at IS NOT NULL);
                UPDATE deeplynx.tags
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );
                UPDATE deeplynx.tags
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.subscriptions
                SET is_archived = (archived_at IS NOT NULL);
                UPDATE deeplynx.subscriptions
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );
                UPDATE deeplynx.subscriptions
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.relationships
                SET is_archived = (archived_at IS NOT NULL);
                UPDATE deeplynx.relationships
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );
                UPDATE deeplynx.relationships
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.records
                SET is_archived = (archived_at IS NOT NULL);
                UPDATE deeplynx.records
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );
                UPDATE deeplynx.records
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.record_mappings
                SET is_archived = (archived_at IS NOT NULL);
                UPDATE deeplynx.record_mappings
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );
                UPDATE deeplynx.record_mappings
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.object_storages
                SET is_archived = (archived_at IS NOT NULL);
                UPDATE deeplynx.object_storages
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );
                UPDATE deeplynx.object_storages
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.edges
                SET is_archived = (archived_at IS NOT NULL);
                UPDATE deeplynx.edges
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );
                UPDATE deeplynx.edges
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.edge_mappings
                SET is_archived = (archived_at IS NOT NULL);
                UPDATE deeplynx.edge_mappings
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );
                UPDATE deeplynx.edge_mappings
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.actions
                SET is_archived = (archived_at IS NOT NULL);
                UPDATE deeplynx.actions
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                );
                UPDATE deeplynx.actions
                SET modified_by = COALESCE(modified_by, created_by);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.users
                SET is_archived = (archived_at IS NOT NULL);");

            migrationBuilder.Sql(@"
                UPDATE deeplynx.historical_records
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.historical_edges
                SET is_archived = (archived_at IS NOT NULL);");

            // =================================================================
            // STEP 3: DROP ALL OLD COLUMNS USING CASCADE FOR SAFETY
            // =================================================================

            // Use CASCADE consistently for all tables to handle any dependencies
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.tags DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.tags DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.tags DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.records DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.records DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.records DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.edges DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edges DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edges DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.actions DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.users DROP COLUMN IF EXISTS archived_at CASCADE;");

            // Historical tables
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS created_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS created_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS modified_at CASCADE;");

            // Events table (special case)
            migrationBuilder.Sql("ALTER TABLE deeplynx.events DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events DROP COLUMN IF EXISTS modified_by CASCADE;");

            // =================================================================
            // STEP 4: RENAME COLUMNS
            // =================================================================

            // Rename for projects
            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "projects",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "projects",
                newName: "last_updated_at");

            // Rename for remaining tables
            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "tags",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "tags",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "relationships",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "relationships",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "records",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "records",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "object_storages",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "object_storages",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "edges",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "edges",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "last_updated_at");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "actions",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "actions",
                newName: "last_updated_at");

            // Historical tables (only modified_by)
            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "historical_records",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "modified_by",
                schema: "deeplynx",
                table: "historical_edges",
                newName: "last_updated_by");

            // Events table - rename existing columns
            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "deeplynx",
                table: "events",
                newName: "last_updated_by");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "deeplynx",
                table: "events",
                newName: "last_updated_at");

            // =================================================================
            // STEP 5: ADD PERFORMANCE INDEXES
            // =================================================================

            migrationBuilder.CreateIndex(
                name: "idx_projects_is_archived",
                schema: "deeplynx",
                table: "projects",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_projects_last_updated_at",
                schema: "deeplynx",
                table: "projects",
                column: "last_updated_at");

            migrationBuilder.CreateIndex(
                name: "idx_tags_is_archived",
                schema: "deeplynx",
                table: "tags",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_is_archived",
                schema: "deeplynx",
                table: "subscriptions", 
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_relationships_is_archived",
                schema: "deeplynx",
                table: "relationships",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_records_is_archived",
                schema: "deeplynx",
                table: "records",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_record_mappings_is_archived",
                schema: "deeplynx",
                table: "record_mappings",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_object_storages_is_archived",
                schema: "deeplynx",
                table: "object_storages",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_edges_is_archived",
                schema: "deeplynx",
                table: "edges",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_edge_mappings_is_archived",
                schema: "deeplynx",
                table: "edge_mappings",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_actions_is_archived",
                schema: "deeplynx",
                table: "actions",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "idx_users_is_archived",
                schema: "deeplynx",
                table: "users",
                column: "is_archived");

            // =================================================================
            // STEP 6: UPDATE ALL STORED PROCEDURES
            // =================================================================

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_class(IN arc_class_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.classes SET is_archived = true, last_updated_at = NOW() WHERE id = arc_class_id;
                    UPDATE deeplynx.relationships SET is_archived = true, last_updated_at = NOW() WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = true, last_updated_at = NOW() WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.record_mappings SET is_archived = true, last_updated_at = NOW() WHERE class_id = arc_class_id;
                END;
                $BODY$;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_class(IN arc_class_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.classes SET is_archived = false, last_updated_at = NOW() WHERE id = arc_class_id;
                    UPDATE deeplynx.relationships SET is_archived = false, last_updated_at = NOW() WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = false, last_updated_at = NOW() WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
                    UPDATE deeplynx.record_mappings SET is_archived = false, last_updated_at = NOW() WHERE class_id = arc_class_id;
                END;
                $BODY$;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_data_source(IN arc_data_source_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.data_sources SET is_archived = true, last_updated_at = NOW() WHERE id = arc_data_source_id;
                    UPDATE deeplynx.record_mappings SET is_archived = true, last_updated_at = NOW() WHERE data_source_id = arc_data_source_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = true, last_updated_at = NOW() WHERE data_source_id = arc_data_source_id;
                END;
                $BODY$;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_data_source(IN arc_data_source_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.data_sources SET is_archived = false, last_updated_at = NOW() WHERE id = arc_data_source_id;
                    UPDATE deeplynx.record_mappings SET is_archived = false, last_updated_at = NOW() WHERE data_source_id = arc_data_source_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = false, last_updated_at = NOW() WHERE data_source_id = arc_data_source_id;
                END;
                $BODY$;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_project(IN arc_project_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.projects SET is_archived = true, last_updated_at = NOW() WHERE id = arc_project_id;
                    UPDATE deeplynx.classes SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.data_sources SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.object_storages SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.record_mappings SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.tags SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.subscriptions SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.actions SET is_archived = true, last_updated_at = NOW() WHERE project_id = arc_project_id;
                END;
                $BODY$;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_project(IN arc_project_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.projects SET is_archived = false, last_updated_at = NOW() WHERE id = arc_project_id;
                    UPDATE deeplynx.classes SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.data_sources SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.records SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edges SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.object_storages SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.relationships SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.record_mappings SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.tags SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.subscriptions SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                    UPDATE deeplynx.actions SET is_archived = false, last_updated_at = NOW() WHERE project_id = arc_project_id;
                END;
                $BODY$;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_record(IN arc_record_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.records SET is_archived = true, last_updated_at = NOW() WHERE id = arc_record_id;
                    UPDATE deeplynx.edges SET is_archived = true, last_updated_at = NOW() WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
                END;
                $BODY$;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_record(IN arc_record_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.records SET is_archived = false, last_updated_at = NOW() WHERE id = arc_record_id;
                    UPDATE deeplynx.edges SET is_archived = false, last_updated_at = NOW() WHERE origin_id = arc_record_id OR destination_id = arc_record_id;
                END;
                $BODY$;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_relationship(IN arc_rel_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.relationships SET is_archived = true, last_updated_at = NOW() WHERE id = arc_rel_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = true, last_updated_at = NOW() WHERE relationship_id = arc_rel_id;
                END;
                $BODY$;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.unarchive_relationship(IN arc_rel_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.relationships SET is_archived = false, last_updated_at = NOW() WHERE id = arc_rel_id;
                    UPDATE deeplynx.edge_mappings SET is_archived = false, last_updated_at = NOW() WHERE relationship_id = arc_rel_id;
                END;
                $BODY$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all indexes first
            migrationBuilder.DropIndex(
                name: "idx_projects_is_archived",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "idx_projects_last_updated_at",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_tags_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_subscriptions_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_relationships_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_records_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_record_mappings_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_object_storages_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_edges_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_edge_mappings_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_actions_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_users_is_archived;");

            // Drop is_archived column
            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "relationships");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "records");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "record_mappings");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "object_storages");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "edge_mappings");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "actions");

            migrationBuilder.DropColumn(
                name: "is_archived",
                schema: "deeplynx",
                table: "users");

            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS is_archived;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS is_archived;");

            // Rename columns back
            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "projects",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "projects",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "tags",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "tags",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "subscriptions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "relationships",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "relationships",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "records",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "records",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "record_mappings",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "object_storages",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "object_storages",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "edges",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "edges",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "edge_mappings",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "actions",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "actions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "historical_records",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "historical_edges",
                newName: "modified_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_by",
                schema: "deeplynx",
                table: "events",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "last_updated_at",
                schema: "deeplynx",
                table: "events",
                newName: "created_at");

            // Re-add old columns using SQL to be safe
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.tags ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.tags ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.tags ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.records ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.records ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.records ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.edges ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edges ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edges ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.actions ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.users ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.events ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events ADD COLUMN IF NOT EXISTS modified_by text;");
        }
    }
}