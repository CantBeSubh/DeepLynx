SET search_path TO deeplynx;

INSERT INTO historical_records (properties, data_source_id, data_source_name, project_name, class_name, original_id, name, tags, project_id, class_id) VALUES
('{"name": "series", "genre": "country", "year": 1997}'::jsonb, 6, 6, 2, 'musician', 1, 'series', '["{tag1}"]', 1, 1),
('{"name": "possible", "genre": "pop", "year": 1984}'::jsonb, 6, 6, 2, 'song', 2, 'possible', '["{tag1}"]', 1, 1);


INSERT INTO historical_edges (origin_id, destination_id, relationship_name, data_source_id, project_id) VALUES
(1, 2, 'plays', 1, 1);


INSERT INTO classes (name, project_id) VALUES
('musician', 1),
('song', 2);

INSERT INTO data_sources (name, project_id) VALUES
('mysrc_1', 1);

INSERT INTO tags (name, project_id) VALUES
('{tag1}', 1);

INSERT INTO relationships (name, origin_id, destination_id, project_id) VALUES
('plays', 1, 2, 1);

