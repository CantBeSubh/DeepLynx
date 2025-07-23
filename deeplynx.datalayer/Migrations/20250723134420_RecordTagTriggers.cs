using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deeplynx.datalayer.Migrations
{
    /// <inheritdoc />
    public partial class RecordTagTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // function to insert a historical record on record_tags insert
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION deeplynx.insert_recordtag_historical_record_trigger()
				    RETURNS trigger AS $$
				BEGIN	
					-- Update all other records with the same id to set current = false
					UPDATE deeplynx.historical_records
					SET current = FALSE
					WHERE record_id = NEW.record_id;

					-- Insert the new historical record
					INSERT INTO deeplynx.historical_records (
						record_id, uri, name, description, properties, original_id, 
						class_id, mapping_id, data_source_id, project_id, 
						created_by, created_at, modified_by, modified_at, 
						last_updated_at, tags, current,
						class_name, data_source_name, project_name)
					SELECT 
						NEW.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
						r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
						r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
						LOCALTIMESTAMP AS last_updated_at, coalesce(json_agg(t.name), '[null]'::json), TRUE,
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
					-- Update all other records with the same id to set current = false
					UPDATE deeplynx.historical_records
					SET current = FALSE
					WHERE record_id = OLD.record_id;

					-- Insert the new historical record
					INSERT INTO deeplynx.historical_records (
						record_id, uri, name, description, properties, original_id, 
						class_id, mapping_id, data_source_id, project_id, 
						created_by, created_at, modified_by, modified_at, 
						last_updated_at, tags, current,
						class_name, data_source_name, project_name)
					SELECT 
						OLD.record_id, r.uri, r.name, r.description, r.properties, r.original_id, 
						r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
						r.created_by, r.created_at, r.modified_by, LOCALTIMESTAMP, 
						LOCALTIMESTAMP AS last_updated_at, coalesce(json_agg(t.name), '[null]'::json), TRUE,
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS insert_historical_records ON deeplynx.record_tags;");
	        migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS delete_historical_records ON deeplynx.record_tags;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS deeplynx.insert_recordtag_historical_record_trigger();");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS deeplynx.delete_recordtag_historical_record_trigger();");
        }
    }
}
