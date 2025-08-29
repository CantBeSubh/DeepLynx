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

            // =================================================================
            // STEP 2: TRANSFORM ALL EXISTING DATA (YOUR EXACT RULES)
            // =================================================================

            // Transform all main entity tables
            migrationBuilder.Sql(@"
                -- Classes table
                UPDATE deeplynx.classes
                SET is_archived = COALESCE(
                    CASE WHEN is_archived IS NOT NULL THEN is_archived ELSE (archived_at IS NOT NULL) END,
                    false
                );");

            migrationBuilder.Sql(@"
                -- Data sources table
                UPDATE deeplynx.data_sources
                SET is_archived = COALESCE(
                    CASE WHEN is_archived IS NOT NULL THEN is_archived ELSE (archived_at IS NOT NULL) END,
                    false
                );");

            migrationBuilder.Sql(@"
                -- Projects table
                UPDATE deeplynx.projects
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.projects
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.projects
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Tags table
                UPDATE deeplynx.tags
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.tags
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.tags
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Subscriptions table
                UPDATE deeplynx.subscriptions
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.subscriptions
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.subscriptions
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Relationships table
                UPDATE deeplynx.relationships
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.relationships
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.relationships
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Records table
                UPDATE deeplynx.records
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.records
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.records
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Record mappings table
                UPDATE deeplynx.record_mappings
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.record_mappings
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.record_mappings
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Object storages table
                UPDATE deeplynx.object_storages
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.object_storages
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.object_storages
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Edges table
                UPDATE deeplynx.edges
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.edges
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.edges
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Edge mappings table
                UPDATE deeplynx.edge_mappings
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.edge_mappings
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.edge_mappings
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Actions table
                UPDATE deeplynx.actions
                SET is_archived = (archived_at IS NOT NULL);

                UPDATE deeplynx.actions
                SET created_at = GREATEST(
                    COALESCE(created_at, '1900-01-01'::timestamp),
                    COALESCE(modified_at, '1900-01-01'::timestamp),
                    COALESCE(archived_at, '1900-01-01'::timestamp)
                )
                WHERE created_at IS NOT NULL OR modified_at IS NOT NULL OR archived_at IS NOT NULL;

                UPDATE deeplynx.actions
                SET modified_by = COALESCE(modified_by, created_by)
                WHERE modified_by IS NOT NULL OR created_by IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Users table (only has archived_at)
                UPDATE deeplynx.users
                SET is_archived = (archived_at IS NOT NULL)
                WHERE archived_at IS NOT NULL;");

            migrationBuilder.Sql(@"
                -- Historical tables
                UPDATE deeplynx.historical_records
                SET is_archived = (archived_at IS NOT NULL)
                WHERE archived_at IS NOT NULL;

                UPDATE deeplynx.historical_edges
                SET is_archived = (archived_at IS NOT NULL)
                WHERE archived_at IS NOT NULL;");

            // =================================================================
            // STEP 3: DROP ALL OLD AUDIT COLUMNS WITH CASCADE (HANDLES ALL DEPENDENCIES)
            // =================================================================

            // Drop from ALL tables with CASCADE to handle any dependencies
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
            // STEP 4: RENAME ALL COLUMNS TO NEW AUDIT PATTERN
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
                    
                    -- Historical tables
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_records' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.historical_records RENAME COLUMN modified_by TO last_updated_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_edges' AND column_name = 'modified_by') THEN
                        ALTER TABLE deeplynx.historical_edges RENAME COLUMN modified_by TO last_updated_by;
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
                    
                    -- Add last_updated_at to historical tables if missing
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_records' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.historical_records ADD COLUMN last_updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_edges' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.historical_edges ADD COLUMN last_updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                    END IF;
                    
                    -- Add last_updated_by to historical tables if missing
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_records' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.historical_records ADD COLUMN last_updated_by text;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_edges' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.historical_edges ADD COLUMN last_updated_by text;
                    END IF;
                    
                    -- Add last_updated_at to events if missing
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'events' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.events ADD COLUMN last_updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                    END IF;
                    
                    -- Add last_updated_by to events if missing
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'events' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.events ADD COLUMN last_updated_by text;
                    END IF;
                END $rename_timestamps$;");

            // =================================================================
            // STEP 5: ADD ALL PERFORMANCE INDEXES
            // =================================================================

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_classes_is_archived ON deeplynx.classes(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_classes_last_updated_at ON deeplynx.classes(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_data_sources_is_archived ON deeplynx.data_sources(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_data_sources_last_updated_at ON deeplynx.data_sources(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_projects_is_archived ON deeplynx.projects(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_projects_last_updated_at ON deeplynx.projects(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_tags_is_archived ON deeplynx.tags(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_tags_last_updated_at ON deeplynx.tags(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_subscriptions_is_archived ON deeplynx.subscriptions(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_subscriptions_last_updated_at ON deeplynx.subscriptions(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_relationships_is_archived ON deeplynx.relationships(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_relationships_last_updated_at ON deeplynx.relationships(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_records_is_archived ON deeplynx.records(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_records_last_updated_at ON deeplynx.records(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_record_mappings_is_archived ON deeplynx.record_mappings(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_record_mappings_last_updated_at ON deeplynx.record_mappings(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_object_storages_is_archived ON deeplynx.object_storages(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_object_storages_last_updated_at ON deeplynx.object_storages(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_edges_is_archived ON deeplynx.edges(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_edges_last_updated_at ON deeplynx.edges(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_edge_mappings_is_archived ON deeplynx.edge_mappings(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_edge_mappings_last_updated_at ON deeplynx.edge_mappings(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_actions_is_archived ON deeplynx.actions(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_actions_last_updated_at ON deeplynx.actions(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_users_is_archived ON deeplynx.users(is_archived);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_historical_records_is_archived ON deeplynx.historical_records(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_historical_records_last_updated_at ON deeplynx.historical_records(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_historical_edges_is_archived ON deeplynx.historical_edges(is_archived);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_historical_edges_last_updated_at ON deeplynx.historical_edges(last_updated_at);");
            
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_events_last_updated_at ON deeplynx.events(last_updated_at);");

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
            // =================================================================
            // COMPLETE ROLLBACK - DROP ALL NEW AUDIT INFRASTRUCTURE
            // =================================================================

            // Drop all stored procedures
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_class;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_class;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_data_source;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_data_source;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_project;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_project;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_record;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_record;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.archive_relationship;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS deeplynx.unarchive_relationship;");

            // Drop all new indexes
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_classes_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_classes_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_data_sources_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_data_sources_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_projects_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_projects_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_tags_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_tags_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_subscriptions_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_subscriptions_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_relationships_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_relationships_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_records_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_records_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_record_mappings_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_record_mappings_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_object_storages_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_object_storages_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_edges_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_edges_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_edge_mappings_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_edge_mappings_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_actions_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_actions_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_users_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_historical_records_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_historical_records_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_historical_edges_is_archived;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_historical_edges_last_updated_at;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS deeplynx.idx_events_last_updated_at;");

            // Drop all new audit columns
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

            // Rename columns back to original names
            migrationBuilder.Sql(@"
                DO $restore_original_columns$
                BEGIN
                    -- Rename last_updated_by back to modified_by
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'classes' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.classes RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'data_sources' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.data_sources RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'projects' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.projects RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'tags' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.tags RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'subscriptions' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.subscriptions RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'relationships' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.relationships RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'records' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.records RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'record_mappings' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.record_mappings RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'object_storages' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.object_storages RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edges' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.edges RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edge_mappings' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.edge_mappings RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'actions' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.actions RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_records' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.historical_records RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'historical_edges' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.historical_edges RENAME COLUMN last_updated_by TO modified_by;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'events' AND column_name = 'last_updated_by') THEN
                        ALTER TABLE deeplynx.events RENAME COLUMN last_updated_by TO created_by;
                    END IF;

                    -- Rename last_updated_at back to created_at
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'classes' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.classes RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'data_sources' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.data_sources RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'projects' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.projects RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'tags' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.tags RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'subscriptions' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.subscriptions RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'relationships' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.relationships RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'records' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.records RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'record_mappings' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.record_mappings RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'object_storages' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.object_storages RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edges' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.edges RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'edge_mappings' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.edge_mappings RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'actions' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.actions RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'deeplynx' AND table_name = 'events' AND column_name = 'last_updated_at') THEN
                        ALTER TABLE deeplynx.events RENAME COLUMN last_updated_at TO created_at;
                    END IF;
                END $restore_original_columns$;");

            // Re-add all original audit columns
            migrationBuilder.Sql("ALTER TABLE deeplynx.classes ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.classes ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.classes ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.data_sources ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

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

            // Historical tables
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_records ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS created_by text;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.historical_edges ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");

            // Events table
            migrationBuilder.Sql("ALTER TABLE deeplynx.events ADD COLUMN IF NOT EXISTS archived_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events ADD COLUMN IF NOT EXISTS modified_at timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE deeplynx.events ADD COLUMN IF NOT EXISTS modified_by text;");
        }
    }
    }
