SET search_path TO deeplynx;

-- TODO: ensure we are only referencing projects, datasources, and classes that exist
-- INSERT INTO records (properties, data_source_id, original_id, name, project_id, class_id) VALUES
-- ('{"name": "series", "genre": "country", "year": 1997}'::jsonb, 6, 1, 'series', 1, 1),
-- ('{"name": "possible", "genre": "pop", "year": 1984}'::jsonb, 6, 2, 'possible',  1, 1);
-- 
-- 
-- INSERT INTO edges (origin_id, destination_id, data_source_id, project_id) VALUES
-- (1, 2, 1, 1);
-- 
-- 
-- INSERT INTO classes (name, project_id) VALUES
-- ('musician', 1),
-- ('song', 2);
-- 
-- INSERT INTO data_sources (name, project_id) VALUES
-- ('mysrc_1', 1);
-- 
-- INSERT INTO tags (name, project_id) VALUES
-- ('{tag1}', 1);
-- 
-- INSERT INTO relationships (name, origin_id, destination_id, project_id) VALUES
-- ('plays', 1, 2, 1);

