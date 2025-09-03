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
            // STEP 1: ADD NEW AUDIT COLUMNS TO ALL TABLES
            // =================================================================

            // Add is_archived to all tables (safe for mixed state)
            migrationBuilder.Sql("ALTER TABLE deeplynx.classes ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.tags ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.records ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edges ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.users ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events ADD COLUMN IF NOT EXISTS is_archived boolean NOT NULL DEFAULT false;");

            // =================================================================
            // STEP 2: TRANSFORM EXISTING DATA - Only for tables with archived_at
            // =================================================================

            // Only update is_archived where archived_at exists and has data
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Classes table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'classes' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.classes SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Data sources table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'data_sources' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.data_sources SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Projects table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'projects' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.projects SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Tags table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'tags' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.tags SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Subscriptions table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'subscriptions' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.subscriptions SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Relationships table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'relationships' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.relationships SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Records table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'records' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.records SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Record mappings table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'record_mappings' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.record_mappings SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Object storages table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'object_storages' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.object_storages SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Edges table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edges' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.edges SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Edge mappings table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edge_mappings' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.edge_mappings SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Actions table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'actions' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.actions SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Users table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'users' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.users SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Historical tables
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_records' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.historical_records SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_edges' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.historical_edges SET is_archived = (archived_at IS NOT NULL);
                    END IF;

                    -- Events table
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'events' AND column_name = 'archived_at') THEN
                        UPDATE deeplynx.events SET is_archived = (archived_at IS NOT NULL);
                    END IF;
                END $$;
            ");

            // =================================================================
            // STEP 3: DROP OLD AUDIT COLUMNS WITH CASCADE
            // =================================================================

            migrationBuilder.Sql("ALTER TABLE deeplynx.classes DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.classes DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.classes DROP COLUMN IF EXISTS modified_at CASCADE;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources DROP COLUMN IF EXISTS created_by CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources DROP COLUMN IF EXISTS modified_at CASCADE;");

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

            // Events table
            migrationBuilder.Sql("ALTER TABLE deeplynx.events DROP COLUMN IF EXISTS archived_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events DROP COLUMN IF EXISTS modified_at CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events DROP COLUMN IF EXISTS modified_by CASCADE;");

            // =================================================================
            // STEP 4: RENAME COLUMNS TO NEW AUDIT PATTERN
            // =================================================================

            migrationBuilder.Sql(@"
                DO $rename_all_columns$
                BEGIN
                    -- Rename modified_by to last_updated_by for all main tables
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'classes' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.classes RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'data_sources' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.data_sources RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'projects' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.projects RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'tags' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.tags RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'subscriptions' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.subscriptions RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'relationships' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.relationships RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'records' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.records RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'record_mappings' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.record_mappings RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'object_storages' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.object_storages RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edges' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.edges RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edge_mappings' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.edge_mappings RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'actions' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.actions RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    -- Events (special case - created_by to last_updated_by)
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'events' AND column_name = 'created_by') THEN
                        ALTER TABLE deeplynx.events RENAME COLUMN created_by TO last_updated_by;
                    END IF;
                END $rename_all_columns$;");

            migrationBuilder.Sql(@"
                DO $rename_timestamps$
                BEGIN
                    -- Rename created_at to last_updated_at for all main tables
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'classes' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.classes RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'data_sources' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.data_sources RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'projects' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.projects RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'tags' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.tags RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'subscriptions' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.subscriptions RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'relationships' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.relationships RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'records' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.records RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'record_mappings' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.record_mappings RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'object_storages' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.object_storages RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edges' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.edges RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edge_mappings' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.edge_mappings RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'actions' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.actions RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    -- Events (special case - created_at to last_updated_at)
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'events' AND column_name = 'created_at') THEN
                        ALTER TABLE deeplynx.events RENAME COLUMN created_at TO last_updated_at;
                    END IF;
                    
                    -- Add missing columns to historical tables
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_records' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.historical_records ADD COLUMN last_updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_edges' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.historical_edges ADD COLUMN last_updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_records' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.historical_records ADD COLUMN last_updated_by text;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_edges' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.historical_edges ADD COLUMN last_updated_by text;
                    END IF;
                    
                    -- Add missing columns to events
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'events' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.events ADD COLUMN last_updated_by text;
                    END IF;
                END $rename_timestamps$;");

            // =================================================================
            // STEP 5: ADD SIMPLE INDEXES (NO CONCURRENTLY)
            // =================================================================

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_classes_is_archived ON deeplynx.classes(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_data_sources_is_archived ON deeplynx.data_sources(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_projects_is_archived ON deeplynx.projects(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_relationships_is_archived ON deeplynx.relationships(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_records_is_archived ON deeplynx.records(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_edges_is_archived ON deeplynx.edges(is_archived);");

            // =================================================================
            // STEP 6: ADD STORED PROCEDURES
            // =================================================================

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE deeplynx.archive_class(IN arc_class_id integer)
                LANGUAGE 'plpgsql'
                AS $BODY$
                BEGIN
                    UPDATE deeplynx.classes SET is_archived = true, last_updated_at = NOW() WHERE id = arc_class_id;
                    UPDATE deeplynx.relationships SET is_archived = true, last_updated_at = NOW() WHERE origin_id = arc_class_id OR destination_id = arc_class_id;
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
                    UPDATE deeplynx.record_mappings SET is_archived = false, last_updated_at = NOW() WHERE class_id = arc_class_id;
                END;
                $BODY$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Simple rollback
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_class;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_class;");
            
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_classes_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_data_sources_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_projects_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_relationships_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_records_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_edges_is_archived;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.classes DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.projects DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.tags DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.subscriptions DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.relationships DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.records DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.record_mappings DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.object_storages DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edges DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.edge_mappings DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.actions DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.users DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges DROP COLUMN IF EXISTS is_archived CASCADE;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events DROP COLUMN IF EXISTS is_archived CASCADE;");
        }
    }
}