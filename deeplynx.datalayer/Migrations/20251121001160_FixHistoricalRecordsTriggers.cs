using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class FixHistoricalRecordsTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update the historical_records_trigger_func to include organization_id
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.historical_records_trigger_func()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id,
                        class_id, data_source_id, project_id, organization_id, object_storage_id,
                        last_updated_by, last_updated_at, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id,
                        NEW.class_id, NEW.data_source_id, NEW.project_id, NEW.organization_id, NEW.object_storage_id,
                        NEW.last_updated_by, NEW.last_updated_at, NEW.is_archived,
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
                        r.class_id, r.data_source_id, r.project_id, r.organization_id, r.object_storage_id,
                        r.last_updated_by, r.last_updated_at, r.is_archived, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Update the historical_records_insert_tag_trigger_func to include organization_id
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.historical_records_insert_tag_trigger_func()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id,
                        class_id, data_source_id, project_id, organization_id, object_storage_id,
                        last_updated_by, last_updated_at, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT
                        NEW.record_id, r.uri, r.name, r.description, r.properties, r.original_id,
                        r.class_id, r.data_source_id, r.project_id, r.organization_id, r.object_storage_id,
                        r.last_updated_by, LOCALTIMESTAMP, r.is_archived,
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
                        r.class_id, r.data_source_id, r.project_id, r.organization_id, r.object_storage_id,
                        r.last_updated_by, r.last_updated_at, r.is_archived, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Update the historical_records_delete_tag_trigger_func to include organization_id
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.historical_records_delete_tag_trigger_func()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id,
                        class_id, data_source_id, project_id, organization_id, object_storage_id,
                        last_updated_by, last_updated_at, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT
                        OLD.record_id, r.uri, r.name, r.description, r.properties, r.original_id,
                        r.class_id, r.data_source_id, r.project_id, r.organization_id, r.object_storage_id,
                        r.last_updated_by, LOCALTIMESTAMP, r.is_archived,
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
                        r.class_id, r.data_source_id, r.project_id, r.organization_id, r.object_storage_id,
                        r.last_updated_by, r.last_updated_at, r.is_archived, c.name, d.name, p.name, o.name;
                    RETURN OLD;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Update the historical_edges_trigger_func to include organization_id
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.historical_edges_trigger_func()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id,
                        relationship_id, data_source_id, project_id, organization_id,
                        last_updated_by, last_updated_at, is_archived,
                        relationship_name, data_source_name, project_name)
                    SELECT
                        NEW.id, NEW.origin_id, NEW.destination_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id, NEW.organization_id,
                        NEW.last_updated_by, NEW.last_updated_at, NEW.is_archived,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    LEFT JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to original historical_records_trigger_func (without organization_id)
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.historical_records_trigger_func()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id,
                        class_id, data_source_id, project_id, object_storage_id,
                        last_updated_by, last_updated_at, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT
                        NEW.id, NEW.uri, NEW.name, NEW.description, NEW.properties, NEW.original_id,
                        NEW.class_id, NEW.data_source_id, NEW.project_id, NEW.object_storage_id,
                        NEW.last_updated_by, NEW.last_updated_at, NEW.is_archived,
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
                        r.class_id, r.data_source_id, r.project_id, r.object_storage_id,
                        r.last_updated_by, r.last_updated_at, r.is_archived, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Revert to original historical_records_insert_tag_trigger_func
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.historical_records_insert_tag_trigger_func()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id,
                        class_id, data_source_id, project_id, object_storage_id,
                        last_updated_by, last_updated_at, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT
                        NEW.record_id, r.uri, r.name, r.description, r.properties, r.original_id,
                        r.class_id, r.data_source_id, r.project_id, r.object_storage_id,
                        r.last_updated_by, LOCALTIMESTAMP, r.is_archived,
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
                        r.class_id, r.data_source_id, r.project_id, r.object_storage_id,
                        r.last_updated_by, r.last_updated_at, r.is_archived, c.name, d.name, p.name, o.name;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Revert to original historical_records_delete_tag_trigger_func
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.historical_records_delete_tag_trigger_func()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical record
                    INSERT INTO deeplynx.historical_records (
                        record_id, uri, name, description, properties, original_id,
                        class_id, data_source_id, project_id, object_storage_id,
                        last_updated_by, last_updated_at, is_archived, tags,
                        class_name, data_source_name, project_name, object_storage_name)
                    SELECT
                        OLD.record_id, r.uri, r.name, r.description, r.properties, r.original_id,
                        r.class_id, r.data_source_id, r.project_id, r.object_storage_id,
                        r.last_updated_by, LOCALTIMESTAMP, r.is_archived,
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
                        r.class_id, r.data_source_id, r.project_id, r.object_storage_id,
                        r.last_updated_by, r.last_updated_at, r.is_archived, c.name, d.name, p.name, o.name;
                    RETURN OLD;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Revert to original historical_edges_trigger_func
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.historical_edges_trigger_func()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Insert the new historical edge
                    INSERT INTO deeplynx.historical_edges (
                        edge_id, origin_id, destination_id,
                        relationship_id, data_source_id, project_id,
                        last_updated_by, last_updated_at, is_archived,
                        relationship_name, data_source_name, project_name)
                    SELECT
                        NEW.id, NEW.origin_id, NEW.destination_id,
                        NEW.relationship_id, NEW.data_source_id, NEW.project_id,
                        NEW.last_updated_by, NEW.last_updated_at, NEW.is_archived,
                        r.name, d.name, p.name
                    FROM deeplynx.edges e
                    LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
                    LEFT JOIN deeplynx.data_sources d ON d.id = e.data_source_id
                    JOIN deeplynx.projects p ON p.id = e.project_id
                    WHERE e.id = NEW.id;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
        }
    }
}
