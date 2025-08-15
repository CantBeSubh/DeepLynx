SET search_path TO deeplynx;

-- Insert into classes table
INSERT INTO classes (name, project_id) 
VALUES
('musician', (SELECT id FROM projects where name = 'test' LIMIT 1)),
('song', (SELECT id FROM projects where name = 'test' LIMIT 1));

-- Insert into records table
INSERT INTO records (properties, data_source_id, original_id, name, project_id, class_id, description) 
VALUES
(
    ('{"name": "series", "genre": "country", "year": 1997}'::jsonb),
    (SELECT id FROM data_sources WHERE name = 'test' AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1),
    5,
    'series',
    (SELECT id FROM projects where name = 'test' LIMIT 1),
    (SELECT id FROM classes WHERE name = 'musician' AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1),
    'test'
),
(
    ('{"name": "possible", "genre": "pop", "year": 1984}'::jsonb),
    (SELECT id FROM data_sources WHERE name = 'test' AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1),
    6,
    'possible',
    (SELECT id FROM projects where name = 'test' LIMIT 1),
    (SELECT id FROM classes WHERE name = 'song' AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1),
    'test'
);

-- Insert into relationships table
INSERT INTO relationships (name, origin_id, destination_id, project_id)
VALUES
(
    'plays',
    (SELECT id FROM classes WHERE name = 'musician' AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1),
    (SELECT id FROM classes WHERE name = 'song' AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1),
    (SELECT id FROM projects where name = 'test' LIMIT 1)
);

-- Insert into edges table
INSERT INTO edges (origin_id, destination_id, data_source_id, project_id, relationship_id)
VALUES
(
    (SELECT id FROM records WHERE name = 'series' AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1),
    (SELECT id FROM records WHERE name = 'possible' AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1),
    (SELECT id FROM data_sources WHERE name = 'test' AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1),
    (SELECT id FROM projects where name = 'test' LIMIT 1),
    (SELECT id FROM relationships 
        where origin_id = (SELECT class_id FROM records 
            WHERE name = 'series' AND project_id = (
                SELECT id FROM projects where name = 'test' LIMIT 1) 
            LIMIT 1) 
        AND destination_id = (SELECT class_id FROM records 
            WHERE name = 'possible' AND project_id = (
                SELECT id FROM projects where name = 'test' LIMIT 1)
            LIMIT 1) 
        AND project_id = (SELECT id FROM projects where name = 'test' LIMIT 1) LIMIT 1)
);



