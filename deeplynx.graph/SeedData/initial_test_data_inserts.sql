SET search_path TO deeplynx;
DELETE FROM projects WHERE name = 'test';
-- Insert into projects
INSERT INTO projects (id, name) VALUES (0, 'test'),(1, 'test'), (2, 'test');

-- Insert into data_sources with subquery for project_id
INSERT INTO data_sources (id, name, config, project_id)
VALUES (0, 'test', '{}', (SELECT id FROM projects WHERE name = 'test' LIMIT 1));

-- Insert into tags with subquery for project_id
INSERT INTO tags (name, project_id)
VALUES ('tag1', (SELECT id FROM projects WHERE name = 'test' LIMIT 1)),
		('tag2', (SELECT id FROM projects WHERE name = 'test' LIMIT 1)),
		('tag3', (SELECT id FROM projects WHERE name = 'test' LIMIT 1));

-- Insert into classes with subquery for project_id
INSERT INTO classes (name, project_id)
VALUES ('test', (SELECT id FROM projects WHERE name = 'test' LIMIT 1));

-- Insert into relationships with subquery for origin_id, destination_id, and project_id
INSERT INTO relationships (name, origin_id, destination_id, project_id)
VALUES ('test',
        (SELECT id FROM classes WHERE name = 'test' LIMIT 1),
        (SELECT id FROM classes WHERE name = 'test' LIMIT 1),
        (SELECT id FROM projects WHERE name = 'test' LIMIT 1));

-- Insert into record_mappings with subquery for project_id
INSERT INTO record_mappings (record_params, project_id, data_source_id)
VALUES ('{}', (SELECT id FROM projects WHERE name = 'test' LIMIT 1), (SELECT id FROM data_sources WHERE name='test' LIMIT 1));

-- Insert into edge_mappings with subquery for relationship_id, origin_id, destination_id, and project_id
INSERT INTO edge_mappings (origin_params, destination_params,
relationship_id, origin_id, destination_id, project_id, data_source_id)
VALUES ('{}', '{}',
        (SELECT id FROM relationships WHERE name = 'test' LIMIT 1),
        (SELECT id FROM classes WHERE name = 'test' LIMIT 1),
        (SELECT id FROM classes WHERE name = 'test' LIMIT 1),
        (SELECT id FROM projects WHERE name = 'test' LIMIT 1),
		(SELECT id FROM data_sources WHERE name = 'test' LIMIT 1));

-- Insert into records with subquery for project_id, data_source_id, and class_id
INSERT INTO records (id, name, properties, project_id, data_source_id, class_id)
VALUES (0, 'test', '{}',
        (SELECT id FROM projects WHERE name = 'test' LIMIT 1),
        (SELECT id FROM data_sources WHERE name = 'test' LIMIT 1),
        (SELECT id FROM classes WHERE name = 'test' LIMIT 1));

-- Insert into edges with subquery for origin_id, destination_id, project_id, data_source_id, and relationship_id
INSERT INTO edges (id, origin_id, destination_id, project_id, data_source_id, relationship_id)
VALUES (0, ((SELECT id FROM records WHERE name = 'test' LIMIT 1)),
        ((SELECT id FROM records WHERE name = 'test' LIMIT 1)),
        (SELECT id FROM projects WHERE name = 'test' LIMIT 1),
        (SELECT id FROM data_sources WHERE name = 'test' LIMIT 1),
        (SELECT id FROM relationships WHERE name = 'test' LIMIT 1));

INSERT INTO record_tags (record_id, tag_id) VALUES
((SELECT id FROM records WHERE name = 'test' LIMIT 1), (SELECT id FROM tags WHERE name = 'tag1' LIMIT 1)),
((SELECT id FROM records WHERE name = 'test' LIMIT 1), (SELECT id FROM tags WHERE name = 'tag2' LIMIT 1)),
((SELECT id FROM records WHERE name = 'test' LIMIT 1), (SELECT id FROM tags WHERE name = 'tag3' LIMIT 1));