SET search_path TO deeplynx;

DELETE FROM projects;

-- Insert into projects
INSERT INTO projects (name) VALUES ('test');

-- Insert into data_sources with subquery for project_id
INSERT INTO data_sources (name, config, project_id)
VALUES ('test', '{}', (SELECT id FROM projects WHERE name = 'test' LIMIT 1));

-- Insert into object_storages with subquery for project_id
INSERT INTO object_storages (name, type, config, project_id, "default")
VALUES ('test', 'filesystem', '{}', (SELECT id FROM projects WHERE name = 'test' LIMIT 1), false);

-- Insert into tags with subquery for project_id
INSERT INTO tags (name, project_id)
VALUES ('test', (SELECT id FROM projects WHERE name = 'test' LIMIT 1));

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
VALUES ('{}', (SELECT id FROM projects WHERE name = 'test' LIMIT 1), (SELECT id FROM data_sources WHERE name = 'test' LIMIT 1));

-- Insert into edge_mappings with subquery for relationship_id, origin_id, destination_id, and project_id
INSERT INTO edge_mappings (origin_params, destination_params, relationship_id, origin_id, destination_id, project_id, data_source_id)
VALUES ('{}', '{}',
        (SELECT id FROM relationships WHERE name = 'test' LIMIT 1),
        (SELECT id FROM classes WHERE name = 'test' LIMIT 1),
        (SELECT id FROM classes WHERE name = 'test' LIMIT 1),
        (SELECT id FROM projects WHERE name = 'test' LIMIT 1),
		(SELECT id FROM data_sources WHERE name = 'test' LIMIT 1));

-- Insert into records with subquery for project_id, data_source_id, and class_id
INSERT INTO records (name, description, original_id, properties, project_id, object_storage_id, data_source_id, class_id)
VALUES ('test', 'test', 'test', '{"test": true}',
        (SELECT id FROM projects WHERE name = 'test' LIMIT 1),
        (SELECT id FROM object_storages WHERE name = 'test' LIMIT 1),
        (SELECT id FROM data_sources WHERE name = 'test' LIMIT 1),
        (SELECT id FROM classes WHERE name = 'test' LIMIT 1));

-- Insert into edges with subquery for origin_id, destination_id, project_id, data_source_id, and relationship_id
INSERT INTO edges (origin_id, destination_id, project_id, data_source_id, relationship_id)
VALUES (((SELECT id FROM records WHERE name = 'test' LIMIT 1)),
        ((SELECT id FROM records WHERE name = 'test' LIMIT 1)),
        (SELECT id FROM projects WHERE name = 'test' LIMIT 1),
        (SELECT id FROM data_sources WHERE name = 'test' LIMIT 1),
        (SELECT id FROM relationships WHERE name = 'test' LIMIT 1));

-- insert into record tags
INSERT INTO record_tags (record_id, tag_id) VALUES
((SELECT id FROM records WHERE name = 'test' LIMIT 1), (SELECT id FROM tags WHERE name = 'test' LIMIT 1));

-- insert into users
INSERT INTO users (name, email, password) VALUES
('test', 'test@test.com', 'password');

-- -- insert into user_projects
-- INSERT INTO user_project (user_id, project_id) VALUES
-- ((SELECT id FROM users WHERE name = 'test' LIMIT 1), (SELECT id FROM projects WHERE name = 'test' LIMIT 1));