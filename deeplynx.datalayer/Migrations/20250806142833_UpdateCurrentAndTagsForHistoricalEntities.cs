using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCurrentAndTagsForHistoricalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_historical_records_current",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropIndex(
                name: "idx_historical_edges_current",
                schema: "deeplynx",
                table: "historical_edges");

            migrationBuilder.DropColumn(
                name: "current",
                schema: "deeplynx",
                table: "historical_records");

            migrationBuilder.DropColumn(
                name: "current",
                schema: "deeplynx",
                table: "historical_edges");
            
            // function to insert a historical record on record_tags insert
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.insert_recordtag_historical_record_trigger()
                 RETURNS trigger AS $$
             BEGIN  
                -- Insert the new historical record
                INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, 
                   created_by, created_at, modified_by, modified_at, 
                   last_updated_at, tags,
                   class_name, data_source_name, project_name)
                SELECT 
                   NEW.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                   r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                   r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
                   LOCALTIMESTAMP AS last_updated_at, 
                   coalesce(json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL), '[null]'::json),
                   c.name, d.name, p.name
                FROM deeplynx.records r
                LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                JOIN deeplynx.projects p ON p.id = r.project_id
                WHERE r.id = NEW.record_id
                GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                      r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                      r.created_by, r.created_at, r.modified_by, r.modified_at, 
                      r.archived_at, c.name, d.name, p.name;
                RETURN NEW;
             END;
             $$ LANGUAGE plpgsql;
            ");
            
            // apply function on insert
            migrationBuilder.Sql(@"
             CREATE OR REPLACE TRIGGER insert_historical_records
              AFTER INSERT ON deeplynx.record_tags
              FOR EACH ROW
              EXECUTE FUNCTION deeplynx.insert_recordtag_historical_record_trigger();
          ");
            
            // function to insert a historical record on record_tags delete
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.delete_recordtag_historical_record_trigger()
                 RETURNS trigger AS $$
             BEGIN  
                -- Insert the new historical record
                INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, 
                   created_by, created_at, modified_by, modified_at, 
                   last_updated_at, tags,
                   class_name, data_source_name, project_name)
                SELECT 
                   OLD.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                   r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                   r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
                   LOCALTIMESTAMP AS last_updated_at, 
                   coalesce(json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL), '[null]'::json),
                   c.name, d.name, p.name
                FROM deeplynx.records r
                LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                JOIN deeplynx.projects p ON p.id = r.project_id
                WHERE r.id = OLD.record_id
                GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                      r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                      r.created_by, r.created_at, r.modified_by, r.modified_at, 
                      r.archived_at, c.name, d.name, p.name;
                RETURN OLD;
             END;
             $$ LANGUAGE plpgsql;
            ");
            
            // apply function on delete
            migrationBuilder.Sql(@"
             CREATE OR REPLACE TRIGGER delete_historical_records
              AFTER DELETE ON deeplynx.record_tags
              FOR EACH ROW
              EXECUTE FUNCTION deeplynx.delete_recordtag_historical_record_trigger();
          ");
            
            // method to automatically set modified_at when we make updates to rows
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_modified_at()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.modified_at = CURRENT_TIMESTAMP;
                    RETURN NEW;
                END;
                $$ language 'plpgsql';
            ");

            // apply the modified_at method to records and edges
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER update_modified_at_records
                BEFORE UPDATE ON deeplynx.records
                FOR EACH ROW
                EXECUTE FUNCTION deeplynx.update_modified_at();
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER update_modified_at_edges
                BEFORE UPDATE ON deeplynx.edges
                FOR EACH ROW
                EXECUTE FUNCTION deeplynx.update_modified_at();
            ");

            // function to update historical records on record creation
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.create_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, 
                   created_by, created_at,
                   last_updated_at, tags,
                   class_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, 
                        NEW.created_by, NEW.created_at,
                        NEW.created_at AS last_updated_at, 
                        coalesce(json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL), '[null]'::json),
                        c.name, d.name, p.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // apply creation function on insert
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER create_historical_records
                AFTER INSERT ON deeplynx.records
                FOR EACH ROW
                EXECUTE FUNCTION deeplynx.create_historical_records_trigger();
            ");

            // function to update historical records on update
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, 
                        created_by, created_at, modified_by, modified_at, 
                        last_updated_at, tags,
                        class_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, 
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.modified_at AS last_updated_at, 
                        coalesce(json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL), '[null]'::json),
                        c.name, d.name, p.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // apply update function on update, but only when archived_at is not being affected
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER update_historical_records
                AFTER UPDATE ON deeplynx.records
                FOR EACH ROW
                WHEN (NEW.archived_at IS NOT DISTINCT FROM OLD.archived_at)
                EXECUTE FUNCTION deeplynx.update_historical_records_trigger();
             ");

            // function to update historical records on archive
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.archive_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, 
                        created_by, created_at, modified_by, modified_at, 
                        archived_at, last_updated_at, tags,
                        class_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, 
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.archived_at, NEW.archived_at AS last_updated_at, 
                        coalesce(json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL), '[null]'::json),
                        c.name, d.name, p.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // apply archive function on update, but only when archived_at is being set to not null
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER archive_historical_records
                AFTER UPDATE ON deeplynx.records
                FOR EACH ROW
                WHEN (NEW.archived_at IS NOT NULL AND NEW.archived_at IS DISTINCT FROM OLD.archived_at)
                EXECUTE FUNCTION deeplynx.archive_historical_records_trigger();
            ");

            // function to update historical records on unarchive
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.unarchive_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, 
                        created_by, created_at, modified_by, modified_at, 
                        archived_at, last_updated_at, tags,
                        class_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, 
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.archived_at, NEW.modified_at AS last_updated_at, 
                        coalesce(json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL), '[null]'::json),
                        c.name, d.name, p.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // apply unarchive function on update, but only when archived_at is being set to null
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER unarchive_historical_records
                AFTER UPDATE ON deeplynx.records
                FOR EACH ROW
                WHEN (NEW.archived_at IS NULL AND NEW.archived_at IS DISTINCT FROM OLD.archived_at)
                EXECUTE FUNCTION deeplynx.unarchive_historical_records_trigger();
            ");

            // function to update historical edges on edge creation
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.create_historical_edges_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        created_at, created_by, last_updated_at,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.created_at, NEW.created_by, 
                        NEW.created_at AS last_updated_at,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // apply creation function on insert
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER create_historical_edges
                AFTER INSERT ON deeplynx.edges
                FOR EACH ROW
                EXECUTE FUNCTION deeplynx.create_historical_edges_trigger();
            ");

            // function to update historical edges on update
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_historical_edges_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        created_at, created_by, modified_at, modified_by,
                        last_updated_at,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.created_at, NEW.created_by, NEW.modified_at, NEW.modified_by, 
                        NEW.modified_at AS last_updated_at,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // apply update function on update, but only when archived_at is not being affected
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER update_historical_edges
                AFTER UPDATE ON deeplynx.edges
                FOR EACH ROW
                WHEN (NEW.archived_at IS NOT DISTINCT FROM OLD.archived_at)
                EXECUTE FUNCTION deeplynx.update_historical_edges_trigger();
                ");

            // function to update historical edges on archive
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.archive_historical_edges_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        created_at, created_by, modified_at, modified_by,
                        archived_at, last_updated_at,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.created_at, NEW.created_by, NEW.modified_at, NEW.modified_by, 
                        NEW.archived_at, NEW.archived_at AS last_updated_at,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // apply archive function on update, but only when archived_at is being set to not null
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER archive_historical_edges
                AFTER UPDATE ON deeplynx.edges
                FOR EACH ROW
                WHEN (NEW.archived_at IS NOT NULL AND NEW.archived_at IS DISTINCT FROM OLD.archived_at)
                EXECUTE FUNCTION deeplynx.archive_historical_edges_trigger();
            ");

            // function to update historical edges on unarchive
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.unarchive_historical_edges_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id, mapping_id,
                        relationship_id, data_source_id, project_id,
                        created_at, created_by, modified_at, modified_by,
                        archived_at, last_updated_at,
                        relationship_name, data_source_name, project_name)
                    SELECT 
                        NEW.id, NEW.origin_id, NEW.destination_id, NEW.mapping_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.created_at, NEW.created_by, NEW.modified_at, NEW.modified_by, 
                        NEW.archived_at, NEW.modified_at AS last_updated_at,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // apply unarchive function on update, but only when archived_at is being set to null
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER unarchive_historical_edges
                AFTER UPDATE ON deeplynx.edges
                FOR EACH ROW
                WHEN (NEW.archived_at IS NULL AND NEW.archived_at IS DISTINCT FROM OLD.archived_at)
                EXECUTE FUNCTION deeplynx.unarchive_historical_edges_trigger();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "current",
                schema: "deeplynx",
                table: "historical_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "current",
                schema: "deeplynx",
                table: "historical_edges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "idx_historical_records_current",
                schema: "deeplynx",
                table: "historical_records",
                column: "current");

            migrationBuilder.CreateIndex(
                name: "idx_historical_edges_current",
                schema: "deeplynx",
                table: "historical_edges",
                column: "current");
            
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS insert_historical_records ON deeplynx.record_tags;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS delete_historical_records ON deeplynx.record_tags;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS deeplynx.insert_recordtag_historical_record_trigger();");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS deeplynx.delete_recordtag_historical_record_trigger();");
            
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS update_modified_at_records ON deeplynx.records;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS update_modified_at_edges ON deeplynx.edges;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS deeplynx.update_modified_at();");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS create_historical_records ON deeplynx.records;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS deeplynx.create_historical_records_trigger();");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS update_historical_records ON deeplynx.records;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS deeplynx.update_historical_records_trigger();");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS archive_historical_records ON deeplynx.records;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS deeplynx.archive_historical_records_trigger();");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS unarchive_historical_records ON deeplynx.records;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS deeplynx.unarchive_historical_records_trigger();");
        }
    }
}
