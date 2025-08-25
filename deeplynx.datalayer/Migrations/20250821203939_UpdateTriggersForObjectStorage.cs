using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTriggersForObjectStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            
            // function to update historical records on record creation
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.create_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, object_storage_id,
                   created_by, created_at,
                   last_updated_at, tags,
                   class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.created_by, NEW.created_at,
                        NEW.created_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
             // function to insert a historical record on record_tags insert
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.insert_recordtag_historical_record_trigger()
                 RETURNS trigger AS $$
             BEGIN  
                -- Insert the new historical record
                INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, object_storage_id,
                   created_by, created_at, modified_by, modified_at, 
                   last_updated_at, tags,
                   class_name, data_source_name, project_name,  object_storage_name)
                SELECT 
                   NEW.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                   r.class_id, r.mapping_id, r.data_source_id, r.project_id,  r.object_storage_id, 
                   r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
                   LOCALTIMESTAMP AS last_updated_at, 
                   json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                   c.name, d.name, p.name, o.name
                FROM deeplynx.records r
                LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                JOIN deeplynx.projects p ON p.id = r.project_id
                WHERE r.id = NEW.record_id
                GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                      r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                      r.created_by, r.created_at, r.modified_by, r.modified_at, 
                      r.archived_at, c.name, d.name, p.name, o.name;
                RETURN NEW;
             END;
             $$ LANGUAGE plpgsql;
            ");
            
            // function to insert a historical record on record_tags delete
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.delete_recordtag_historical_record_trigger()
                 RETURNS trigger AS $$
             BEGIN  
                -- Insert the new historical record
                INSERT INTO deeplynx.historical_records (
                   record_id, uri, name, description, properties, original_id, 
                   class_id, mapping_id, data_source_id, project_id, object_storage_id,
                   created_by, created_at, modified_by, modified_at, 
                   last_updated_at, tags,
                   class_name, data_source_name, project_name, object_storage_name)
                SELECT 
                   OLD.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
                   r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                   r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
                   LOCALTIMESTAMP AS last_updated_at, 
                   json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                   c.name, d.name, p.name, o.name
                FROM deeplynx.records r
                LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                JOIN deeplynx.projects p ON p.id = r.project_id
                WHERE r.id = OLD.record_id
                GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                      r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                      r.created_by, r.created_at, r.modified_by, r.modified_at, 
                      r.archived_at, c.name, d.name, p.name,  o.name;
                RETURN OLD;
             END;
             $$ LANGUAGE plpgsql;
            ");
            
             // function to update historical records on update
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.update_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        last_updated_at, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, New.object_storage_id,
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.modified_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
             // function to update historical records on archive
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.archive_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        archived_at, last_updated_at, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, New.object_storage_id,
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.archived_at, NEW.archived_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            // function to update historical records on unarchive
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.unarchive_historical_records_trigger()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id, 
                        class_id, mapping_id, data_source_id, project_id, object_storage_id,
                        created_by, created_at, modified_by, modified_at, 
                        archived_at, last_updated_at, tags,
                        class_name, data_source_name, project_name,  object_storage_name)
                    SELECT 
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id, 
                        NEW.class_id, NEW.mapping_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.created_by, NEW.created_at, NEW.modified_by, NEW.modified_at, 
                        NEW.archived_at, NEW.modified_at AS last_updated_at, 
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
                        c.name, d.name, p.name, o.name
                    FROM deeplynx.records r
                    LEFT JOIN deeplynx.record_tags rt ON r.id = rt.record_id
                    LEFT JOIN deeplynx.tags t ON t.id = rt.tag_id
                    LEFT JOIN deeplynx.classes c ON c.id = r.class_id
                    LEFT JOIN deeplynx.object_storages o ON o.id = r.object_storage_id
                    JOIN deeplynx.data_sources d ON d.id = r.data_source_id
                    JOIN deeplynx.projects p ON p.id = r.project_id
                    WHERE r.id = NEW.id
                    GROUP BY r.id, r.uri, r.name, r.description, r.properties, r.original_id, 
                            r.class_id, r.mapping_id, r.data_source_id, r.project_id, r.object_storage_id,
                            r.created_by, r.created_at, r.modified_by, r.modified_at, 
                            r.archived_at, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                   json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
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
                   json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
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
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
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
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
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
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
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
                        json_agg(jsonb_build_object('id', t.id, 'name', t.name)) FILTER (WHERE t.id IS NOT NULL AND t.name IS NOT NULL),
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
        }
    }
}
