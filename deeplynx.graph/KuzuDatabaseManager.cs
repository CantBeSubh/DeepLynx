using System;
using static kuzunet;
using System.Text;
using System.Data;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.RegularExpressions;

namespace deeplynx.graph
{
    public class KuzuDatabaseManager : IKuzuDatabaseManager
    {
        private readonly string _kuzuDbPath = Path.GetFullPath("../deeplynx.graph/kuzu_db");
        private kuzu_database? _db;
        private kuzu_connection? _conn;
        private bool _isDatabaseInitialized = false;
        private readonly string _pgParams;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private readonly string _connectionString;
        private readonly string _tenantId;
        private List<string>? _cachedClassNames;
        private List<RelationshipInfo>? _cachedRelationshipNames;


        /// <summary>
        /// Initializes a new instance of the KuzuDatabaseManager class.
        /// </summary>
        public KuzuDatabaseManager(IConfiguration configuration, string? connectionString, string tenantId)
        {
            _db = new kuzu_database();
            _conn = new kuzu_connection();
            _tenantId = tenantId;

            _connectionString = connectionString ?? configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is not configured.");
            _pgParams = TransformConnectionString(_connectionString);
        }


        /// <summary>
        /// Transforms a connection string from the format "User ID=...;Password=...;Database=...;Server=...;Port=..." 
        /// to the format "dbname=... user=... host=... password=... port=...".
        /// </summary>
        /// <param name="input">The original connection string to transform.</param>
        /// <returns>The transformed connection string.</returns>
        private static string TransformConnectionString(string input)
        {
            var keyValuePairs = input.Split([';'], StringSplitOptions.RemoveEmptyEntries);
            var dictionary = new Dictionary<string, string>();

            foreach (var pair in keyValuePairs)
            {
                var parts = pair.Split(['='], 2);
                if (parts.Length == 2)
                {
                    dictionary[parts[0].Trim()] = parts[1].Trim();
                }
            }

            // Build the transformed connection string
            var transformed = $"dbname={dictionary["Database"]} " +
                   $"user={(dictionary.TryGetValue("User ID", out string? value1) ? value1 : dictionary["Username"])} " +
                   $"host={(dictionary.TryGetValue("Server", out string? value) ? value : dictionary["Host"])} " +
                   $"password={dictionary["Password"]} " +
                   $"port={dictionary["Port"]}";

            return transformed;
        }


        /// <summary>
        /// Connects to the Kuzu database asynchronously.
        /// Creates the database if it hasn't been initialized yet.
        /// </summary>
        /// <returns>A task representing the asynchronous connection operation.</returns>
        public async Task<bool> ConnectAsync()
        {
            Console.WriteLine("Getting connect lock");
            await _connectionLock.WaitAsync();
            Console.WriteLine("Connect lock acquired");

            try
            {
                kuzu_system_config config = kuzu_default_system_config();

                if (!_isDatabaseInitialized)
                {
                    var state = await Task.Run(() => kuzu_database_init(_kuzuDbPath, config, _db));

                    if (state == kuzu_state.KuzuError)
                    {
                        Console.WriteLine("Could not create DB");
                        return false;
                    }

                    _isDatabaseInitialized = true;
                }

                var connectionState = await Task.Run(() => kuzu_connection_init(_db, _conn));
                if (connectionState == kuzu_state.KuzuError)
                {
                    Console.WriteLine("Could not connect to DB");
                    return false;
                }

                Console.WriteLine("Connected to Kuzu database.");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while connecting to the database: {e.Message}");
                return false;
            }
            finally
            {
                _connectionLock.Release();
                Console.WriteLine("Released connect lock");
            }
        }


        private async Task<int> ExecuteNonQuerySqlAsync(string sqlCommand)
        {

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand(sqlCommand, connection))
                    {
                        var result = await command.ExecuteNonQueryAsync();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database operation failed: {ex.Message}");
                    return 0;
                }
            }
        }

        /// <summary>
        /// Installs the necessary PostgreSQL extensions for KuzuDB.
        /// </summary>
        /// <param name="pgParams">The connection parameters for the PostgreSQL database.</param>
        /// <returns>Returns false if an error occurred and true if not.</returns>
        private async Task<bool> InstallExtensionsAsync()
        {
            if (AreExtensionsAvailableOnDisk())
            {
                Console.WriteLine("Postgres and Json extensions are already installed");
                return true;
            }

            Console.WriteLine("Installing Postgres and JSON extensions.");
            try
            {
                if (_conn == null)
                {
                    await ConnectAsync();
                }

                await Task.Run(() => PerformNonQueryAsync("INSTALL postgres;"));
                await Task.Run(() => PerformNonQueryAsync("INSTALL json;"));

                Console.WriteLine("Installed Postgres and JSON extensions.");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred when installing the Postgres and JSON extensions: {e}");
                return false;
            }
        }


        private bool AreExtensionsAvailableOnDisk()
        {
            var extensionNames = new List<string>
            {
                "libpostgres.kuzu_extension",
                "libjson.kuzu_extension"
            };

            string userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string baseDirectory = Path.Combine(userHomeDirectory, ".kuzu", "extension");

            bool allExtensionsAvailable = true;

            foreach (var extensionName in extensionNames)
            {
                string? extensionPath = FindExtensionPath(baseDirectory, extensionName);
                if (extensionPath == null)
                {
                    Console.WriteLine($"{extensionName} not found in {baseDirectory} or its subdirectories.");
                    allExtensionsAvailable = false;
                }
                else
                {
                    Console.WriteLine($"{extensionName} found at: {extensionPath}");
                }
            }

            return allExtensionsAvailable;
        }

        /// <summary>
        /// Searches for the extension file in the specified directory and its subdirectories.
        /// </summary>
        /// <param name="directory">The directory to search in.</param>
        /// <param name="fileName">The name of the file to find.</param>
        /// <returns>The full path of the file if found; otherwise, null.</returns>
        private static string? FindExtensionPath(string directory, string fileName)
        {
            try
            {
                var files = Directory.GetFiles(directory, fileName, SearchOption.AllDirectories);
                return files.Length > 0 ? files[0] : null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while searching for {fileName}: {e.Message}");
                return null;
            }
        }


        /// <summary>
        /// Installs the necessary PostgreSQL extensions for KuzuDB.
        /// </summary>
        /// <param name="pgParams">The connection parameters for the PostgreSQL database.</param>
        /// <returns>Returns false if an error occurred and true if not.</returns>
        public async Task<bool> LoadExtensionsAsync()
        {
            Console.WriteLine("Loading Postgres and JSON extensions.");
            try
            {
                if (_conn == null)
                {
                    await ConnectAsync();
                }

                await Task.Run(() => PerformNonQueryAsync("LOAD EXTENSION postgres;"));
                await Task.Run(() => PerformNonQueryAsync("LOAD EXTENSION json;"));

                Console.WriteLine("Loaded Postgres and JSON extensions.");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred when loading the Postgres and JSON extensions: {e}");
                return false;
            }
        }


        /// <summary>
        /// Main export function that checks records and edges, assigns defaults, copies data, and indicates successful export.
        /// </summary>
        /// <param name="pgParams">The connection parameters for the PostgreSQL database.</param>
        /// <param name="project_id">The project identifier.</param>
        /// <returns>A task representing the asynchronous export operation.</returns>
        public async Task<bool> ExportDataAsync(int project_id)
        {

            Console.WriteLine("Attempting to acquire export data lock...");
            await _connectionLock.WaitAsync();
            Console.WriteLine("Export Data lock acquired.");

            try
            {
                if (_conn == null)
                {
                    await ConnectAsync();
                }

                bool hasError = false;

                string attachCommand = $"ATTACH '{_pgParams}' AS test (dbtype postgres, skip_unsupported_table = TRUE, schema = 'deeplynx');";

                await CreateHistorical_RecordsView();

                await CreateEdges_CView();

                await InstallExtensionsAsync();

                await LoadExtensionsAsync();

                await PerformNonQueryAsync(attachCommand);

                await PerformNonQueryAsync($"CREATE NODE TABLE IF NOT EXISTS {_tenantId}_ProcessedProjectIds (project_id INT64 PRIMARY KEY);");

                bool projectIdExists = await CheckIfProjectIdExistsAsync(project_id);

                if (projectIdExists)
                {
                    Console.WriteLine($"Project ID {project_id} has already been processed. Skipping data load.");
                    await SyncRecordsAsync(project_id);
                    return false;
                }

                await SetupKuzuTablesAsync();

                hasError = await LoadDataAsync(project_id);

                if (!hasError)
                {
                    await CreateProjectIdNodeAsync(project_id);
                    await UpdateDefaultNamesAsync();
                }

                if (!hasError)
                {
                    Console.WriteLine("Data export completed.");
                    return false;
                }

                return hasError;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred during the export: {e.Message}");
                return false;
            }
            finally
            {
                _connectionLock.Release();
                Console.WriteLine("Export Lock Released");
            }
        }


        /// <summary>
        /// Checks if a specific project ID exists in the ProcessedProjectIds table.
        /// </summary>
        /// <param name="projectId">The project ID to check for existence.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the project ID exists, otherwise false.</returns>
        private async Task<bool> CheckIfProjectIdExistsAsync(long projectId)
        {
            try
            {
                var query = $"MATCH (p:{_tenantId}_ProcessedProjectIds) WHERE p.project_id = {projectId} RETURN COUNT(p) > 0;";
                var requestDto = new KuzuDBMQueryRequestDto { Query = query };
                (string formattedString, object[] results) = await ExecuteQueryAsync(requestDto, false);

                return formattedString.Contains("True");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while checking for project ID existence: {e.Message}");
                return false;
            }
        }

        private async Task EnsureTableExistsAsync(string? className)
        {
            try
            {
                var query = $"CREATE NODE TABLE IF NOT EXISTS {_tenantId}_{className} (record_id INT64 PRIMARY KEY, last_updated_at TIMESTAMP, id INT64, class_name STRING, project_name STRING, project_id INT64, name STRING, uri STRING, data_source_id INT64, original_id STRING, class_id INT64, properties STRING, tags STRING, created_at TIMESTAMP, modified_at TIMESTAMP);";

                await ExecuteQueryAsync(new KuzuDBMQueryRequestDto { Query = query }, false);
                Console.WriteLine($"Ensured that the table {className} exists or was created successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while ensuring the existence of table {className}: {e.Message}");
            }
        }


        /// <summary>
        /// Creates a project ID node in the ProcessedProjectIds table.
        /// </summary>
        /// <param name="projectId">The project ID to insert as a node.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CreateProjectIdNodeAsync(int projectId)
        {
            try
            {
                var query = $"CREATE (p:{_tenantId}_ProcessedProjectIds {{project_id: {projectId}}});";
                await PerformNonQueryAsync(query);
                Console.WriteLine($"Project ID {projectId} inserted into ProcessedProjectIds.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while inserting project ID: {e.Message}");
            }
        }

        private async Task CreateEdges_CView()
        {
            try
            {
                var query = $@"
                    SET search_path TO deeplynx;
                    CREATE OR REPLACE VIEW edges_c AS
                    SELECT c_o.name AS orig_class,
                        e.origin_id,
                        c_d.name AS dest_class,
                        e.destination_id,
                        e.relationship_name,
                        o.project_id,
                        e.id,
                        e.last_updated_at
                    FROM deeplynx.historical_edges e
                        JOIN deeplynx.historical_records_c o ON o.record_id = e.origin_id
                        JOIN deeplynx.historical_records_c d ON d.record_id = e.destination_id
                        JOIN deeplynx.classes c_o ON c_o.id = o.class_id
                        JOIN deeplynx.classes c_d ON c_d.id = d.class_id
                    WHERE o.project_id = d.project_id AND c_o.name <> 'test'::text AND c_d.name <> 'test'::text AND e.relationship_name <> 'test'::text;
                    ";
                await ExecuteNonQuerySqlAsync(query);
                Console.WriteLine($"Created the Edges_c view");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while creating the edges_c view: {e.Message}");
            }
        }

        private async Task<List<KuzuDBMResponseDto>?> GetRecordsFromPostgresAsync(int projectId)
        {
            try
            {
                var query = $@"
                    SELECT 
                        record_id, 
                        last_updated_at, 
                        id, 
                        class_name, 
                        project_name, 
                        project_id,
                        name, 
                        uri, 
                        data_source_id, 
                        original_id, 
                        class_id, 
                        properties, 
                        tags, 
                        created_at, 
                        modified_at 
                    FROM deeplynx.historical_records_c 
                    WHERE project_id = {projectId};";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            List<KuzuDBMResponseDto> records = [];

                            while (await reader.ReadAsync())
                            {
                                KuzuDBMResponseDto recordInfo = new()
                                {
                                    RecordId = reader.GetInt64(reader.GetOrdinal("record_id")),
                                    LastUpdatedAt = reader.GetDateTime(reader.GetOrdinal("last_updated_at")),
                                    Id = reader.GetInt64(reader.GetOrdinal("id")),
                                    ClassName = reader.GetString(reader.GetOrdinal("class_name")),
                                    ProjectName = reader.GetString(reader.GetOrdinal("project_name")),
                                    ProjectId = reader.GetInt64(reader.GetOrdinal("project_id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    Uri = reader.IsDBNull(reader.GetOrdinal("uri")) ? null : reader.GetString(reader.GetOrdinal("uri")),
                                    DataSourceId = reader.GetInt64(reader.GetOrdinal("data_source_id")),
                                    OriginalId = reader.GetString(reader.GetOrdinal("original_id")),
                                    ClassId = reader.GetInt64(reader.GetOrdinal("class_id")),
                                    Properties = reader.IsDBNull(reader.GetOrdinal("properties")) ? null : reader.GetString(reader.GetOrdinal("properties")),
                                    Tags = reader.IsDBNull(reader.GetOrdinal("tags")) ? null : reader.GetString(reader.GetOrdinal("tags")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                                    ModifiedAt = reader.IsDBNull(reader.GetOrdinal("modified_at")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("modified_at")),
                                };

                                records.Add(recordInfo);
                            }

                            return records;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while getting records from postgres database: {e.Message}");
                return null;
            }
        }


        private async Task<List<EdgeDto>?> GetEdgesFromPostgresAsync(int projectId)
        {
            try
            {
                string query = $@"
                    SELECT 
                        orig_class AS OriginClass,
                        origin_id AS OriginId,
                        dest_class AS DestinationClass,
                        destination_id AS DestinationId,
                        relationship_name AS RelationshipName,
                        project_id AS ProjectId,
                        id AS EdgeId
                    FROM deeplynx.edges_c
                    WHERE project_id = {projectId};";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            List<EdgeDto> edges = new();

                            while (await reader.ReadAsync())
                            {
                                EdgeDto edgeInfo = new()
                                {
                                    OriginClass = reader.GetString(reader.GetOrdinal("OriginClass")),
                                    OriginId = reader.GetInt32(reader.GetOrdinal("OriginId")),
                                    DestinationClass = reader.GetString(reader.GetOrdinal("DestinationClass")),
                                    DestinationId = reader.GetInt32(reader.GetOrdinal("DestinationId")),
                                    RelationshipName = reader.GetString(reader.GetOrdinal("RelationshipName")),
                                    ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                                    EdgeId = reader.GetInt32(reader.GetOrdinal("EdgeId")),
                                };

                                edges.Add(edgeInfo);
                            }

                            return edges;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while getting edges from Kuzu database: {e.Message}");
                return null;
            }
        }


        private async Task UpdateRecordInKuzuAsync(KuzuDBMResponseDto record)
        {
            try
            {
                record.ClassName = record?.ClassName?.Replace(" ", "");

                await EnsureTableExistsAsync(record?.ClassName);

                var request = new KuzuDBMQueryRequestDto
                {
                    Query = $@"
                        MATCH (e:{_tenantId}_{record?.ClassName}) 
                        WHERE e.record_id = {record?.RecordId}
                        SET 
                            e.last_updated_at = TIMESTAMP('{record?.LastUpdatedAt:yyyy-MM-dd HH:mm:ss.fffffff}'),
                            e.id = {record?.Id},
                            e.class_name = '{record?.ClassName}',
                            e.project_name = '{record?.ProjectName}',
                            e.project_id = {record?.ProjectId},
                            e.name = '{record?.Name}',
                            e.uri = '{record?.Uri}',
                            e.data_source_id = {record?.DataSourceId},
                            e.original_id = '{record?.OriginalId}',
                            e.class_id = {record?.ClassId},
                            e.properties = '{record?.Properties}',
                            e.tags = '{record?.Tags}',
                            e.created_at = TIMESTAMP('{record?.CreatedAt:yyyy-MM-dd HH:mm:ss.fffffff}'),
                            e.modified_at = TIMESTAMP('{record?.ModifiedAt:yyyy-MM-dd HH:mm:ss.fffffff}')
                        RETURN e;"
                };

                var (formattedString, results) = await ExecuteQueryAsync(request, false);
                Console.WriteLine($"Record {record?.RecordId} has been updated in the {record?.ClassName} table");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while updating record ID {record.RecordId} in Kuzu: {e.Message}");
            }
        }

        private async Task RemoveRecordFromKuzuAsync(long? recordId)
        {
            try
            {
                var request = new KuzuDBMQueryRequestDto
                {
                    Query = $@"
                        MATCH (e) 
                        WHERE e.record_id = {recordId}
                        DELETE e;"
                };

                await ExecuteQueryAsync(request);
                Console.WriteLine($"Record {recordId} has been deleted.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while removing record ID {recordId} from Kuzu: {e.Message}");
            }
        }


        private async Task RemoveEdgeFromKuzuAsync(int? edgeId)
        {
            if (edgeId == null)
            {
                Console.WriteLine("Edge ID is null, cannot remove edge.");
                return;
            }

            try
            {
                string query = $"MATCH ()-[e]->() WHERE id(e) = {edgeId} DELETE e;";
                await ExecuteQueryAsync(new KuzuDBMQueryRequestDto { Query = query });
                Console.WriteLine($"Edge with ID {edgeId} removed from Kuzu successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to remove edge with ID {edgeId}: {e.Message}");
            }
        }


        private async Task AddRecordToKuzuAsync(KuzuDBMResponseDto record)
        {
            try
            {
                record.ClassName = record?.ClassName?.Replace(" ", "");

                await EnsureTableExistsAsync(record?.ClassName);

                var request = new KuzuDBMQueryRequestDto
                {
                    Query = $@"
                        CREATE (e:{_tenantId}_{record?.ClassName} {{ 
                            record_id: {record?.RecordId}, 
                            last_updated_at: TIMESTAMP('{record?.LastUpdatedAt:yyyy-MM-dd HH:mm:ss}'),
                            id: {record?.Id},
                            class_name: '{record?.ClassName}',
                            project_name: '{record?.ProjectName}',
                            project_id: {record?.ProjectId},
                            name: '{record?.Name}',
                            uri: '{record?.Uri}',
                            data_source_id: {record?.DataSourceId},
                            original_id: '{record?.OriginalId}',
                            class_id: {record?.ClassId},
                            properties: '{record?.Properties}',
                            tags: '{record?.Tags}',
                            created_at: TIMESTAMP('{record?.CreatedAt:yyyy-MM-dd HH:mm:ss}'),
                            modified_at: TIMESTAMP('{record?.ModifiedAt:yyyy-MM-dd HH:mm:ss}')
                        }});"
                };

                await ExecuteQueryAsync(request, false);
                Console.WriteLine($"Record {record?.RecordId} has been added to the {record?.ClassName} table");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while adding record ID {record.RecordId} to Kuzu: {e.Message}");
            }
        }


        private async Task AddEdgeToKuzuAsync(EdgeDto edge)
        {
            if (edge == null)
            {
                Console.WriteLine("Edge is null, cannot add edge.");
                return;
            }

            try
            {
                string query = $"MATCH (source), (destination) " +
                            $"WHERE id(source) = {edge.OriginId} AND id(destination) = {edge.DestinationId} " +
                            $"CREATE (source)-[e:{edge.RelationshipName}]->(destination) " +
                            $"SET e.lastUpdatedAt = '{edge.LastUpdatedAt}' ";

                await ExecuteQueryAsync( new KuzuDBMQueryRequestDto { Query = query });
                Console.WriteLine($"Edge added to Kuzu successfully with ID: {edge.EdgeId}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to add edge: {e.Message}");
            }
        }


        private async Task UpdateEdgeInKuzuAsync(EdgeDto edge)
        {
            if (edge == null)
            {
                Console.WriteLine("Edge is null, cannot update edge.");
                return;
            }

            try
            {
                string query = $"MATCH ()-[e]->() WHERE id(e) = {edge.EdgeId} " +
                            $"SET e.lastUpdatedAt = '{edge.LastUpdatedAt}' " +
                            $"RETURN e;";

                var result = await ExecuteQueryAsync(new KuzuDBMQueryRequestDto { Query = query });
                Console.WriteLine($"Edge with ID {edge.EdgeId} updated successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to update edge with ID {edge.EdgeId}: {e.Message}");
            }
        }


        private async Task CreateHistorical_RecordsView()
        {
            try
            {
                var query = $@"
                    SET search_path to deeplynx;
                    CREATE OR REPLACE VIEW historical_records_c AS
                    SELECT DISTINCT ON (record_id)
                        id,
                        record_id,
                        uri,
                        name,
                        properties,
                        original_id,
                        class_id,
                        class_name,
                        mapping_id,
                        data_source_id,
                        data_source_name,
                        project_id,
                        project_name,
                        tags,
                        created_by,
                        created_at,
                        modified_by,
                        modified_at,
                        archived_at,
                        last_updated_at,
                        description
                    FROM 
                        historical_records
                    ORDER BY 
                        record_id, 
                        id DESC;
                    ";
                await ExecuteNonQuerySqlAsync(query);
                Console.WriteLine($"Created the historical_records_c view");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while creating the historical_records_c view: {e.Message}");
            }
        }


        /// <summary>
        /// Dynamically sets up the necessary tables in the Kuzu database.
        /// </summary>
        private async Task SetupKuzuTablesAsync()
        {
            try
            {
                await PerformNonQueryAsync($"CREATE NODE TABLE IF NOT EXISTS {_tenantId}_TableNames (id INT64, class_name STRING, PRIMARY KEY(id));");
                await PerformNonQueryAsync($"COPY {_tenantId}_TableNames FROM (LOAD FROM historical_records_c RETURN id, class_name);");

                await PerformNonQueryAsync($"CREATE NODE TABLE IF NOT EXISTS {_tenantId}_RelTableNames (id INT64, relationship_name STRING, orig_class STRING, dest_class STRING, PRIMARY KEY(id));");
                await PerformNonQueryAsync($"COPY {_tenantId}_RelTableNames FROM (LOAD FROM edges_c RETURN DISTINCT id, relationship_name, orig_class, dest_class);");

                var classNames = await GetUniqueClassNamesAsync();

                foreach (var className in classNames)
                {
                    string createNodeTableQuery = $@"
                        CREATE NODE TABLE {_tenantId}_{CapitalizeFirstLetter(className)} (
                            id INT64,
                            uri STRING,
                            record_id INT64,
                            properties STRING,
                            data_source_id INT64,
                            original_id STRING,
                            class_id INT64,
                            name STRING,
                            mapping_id INT64,
                            class_name STRING,
                            project_id INT64,
                            project_name STRING,
                            data_source_name STRING,
                            tags STRING,
                            created_by STRING,
                            created_at TIMESTAMP,
                            modified_by STRING,
                            modified_at TIMESTAMP,
                            archived_at TIMESTAMP,
                            last_updated_at TIMESTAMP,
                            PRIMARY KEY (record_id)
                        );";

                    await PerformNonQueryAsync(createNodeTableQuery);
                }

                var relationships = await GetUniqueRelationshipNamesAsync();
                var relationshipGroups = relationships.GroupBy(r => r.RelationshipName).ToList();

                if (!relationshipGroups.Any())
                {
                    throw new InvalidOperationException("The edges_c view is empty.");
                }

                foreach (var group in relationshipGroups)
                {
                    var relationshipName = group.Key.ToUpper();
                    var fromToClauses = group.Select(r => $"FROM {_tenantId}_{CapitalizeFirstLetter(r.OrigClass)} TO {_tenantId}_{CapitalizeFirstLetter(r.DestClass)}").ToList();
                    var fromToClause = string.Join(", ", fromToClauses);

                    string createRelTableQuery = $@"
                        CREATE REL TABLE IF NOT EXISTS {_tenantId}_{relationshipName} (
                            {fromToClause},
                            relationship_name STRING
                        );";

                    await PerformNonQueryAsync(createRelTableQuery);
                }

                await PerformNonQueryAsync($@"
                    CREATE NODE TABLE {_tenantId}_Entity (
                        id INT64,
                        uri STRING,
                        record_id INT64,
                        properties STRING,
                        data_source_id INT64,
                        original_id STRING,
                        class_id INT64,
                        name STRING,
                        mapping_id INT64,
                        class_name STRING,
                        project_id INT64,
                        project_name STRING,
                        data_source_name STRING,
                        tags STRING,
                        created_by STRING,
                        created_at TIMESTAMP,
                        modified_by STRING,
                        modified_at TIMESTAMP,
                        archived_at TIMESTAMP,
                        last_updated_at TIMESTAMP,
                        PRIMARY KEY (record_id)
                    );"
                );
                await CreateRelatesToTableAsync();

                Console.WriteLine("Tables setup in KuzuDB.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while setting up Kuzu tables: {e.Message}");
            }
        }


        /// <summary>
        /// Creates the RELATES_TO table dynamically based on unique relationships.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CreateRelatesToTableAsync()
        {
            try
            {
                var relationships = await GetUniqueRelationshipNamesAsync();
                var fromToClauses = new HashSet<string>();

                foreach (var relationship in relationships)
                {
                    string fromToClause = $"FROM {_tenantId}_{CapitalizeFirstLetter(relationship.OrigClass)} TO {_tenantId}_{CapitalizeFirstLetter(relationship.DestClass)}";
                    fromToClauses.Add(fromToClause);
                }

                string combinedFromToClauses = string.Join(", ", fromToClauses);

                string createRelTableQuery = $@"
                    CREATE REL TABLE IF NOT EXISTS {_tenantId}_RELATES_TO (
                        {combinedFromToClauses},
                        relationship_name STRING
                    );";

                await PerformNonQueryAsync(createRelTableQuery);

            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while creating the RELATES_TO table: {e.Message}");
            }
        }


        /// <summary>
        /// Capitalizes the first letter of the input string and converts the rest to lowercase.
        /// </summary>
        /// <param name="input">The string to be modified.</param>
        /// <returns>The modified string with the first letter capitalized and the rest in lowercase. If the input is null or empty, it returns the input as is.</returns>
        private static string CapitalizeFirstLetter(string input)
        {
            input = input.Replace(" ", "_");

            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }


        /// <summary>
        /// Retrieves unique class names from the TableNames table.
        /// </summary>
        /// <returns>A list of unique class names.</returns>
        private async Task<List<string>> GetUniqueClassNamesAsync()
        {
            if (_cachedClassNames != null)
            {
                return _cachedClassNames;
            }

            try
            {
                string query = $"MATCH (t:{_tenantId}_TableNames) WHERE t.class_name IS NOT NULL RETURN DISTINCT t.class_name;";
                var requestDto = new KuzuDBMQueryRequestDto { Query = query };
                (string formattedString, object[] results) = await ExecuteQueryAsync(requestDto);

                List<string> classNames = new();

                var lines = formattedString.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        classNames.Add(trimmedLine);
                    }
                }

                _cachedClassNames = classNames;
                return classNames;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving unique class names: {e.Message}");
                return new List<string>();
            }
        }


        /// <summary>
        /// Retrieves unique relationship names from the RelTableNames table.
        /// </summary>
        /// <returns>A list of unique relationship names.</returns>
        private async Task<List<RelationshipInfo>> GetUniqueRelationshipNamesAsync()
        {
            if (_cachedRelationshipNames != null)
            {
                return _cachedRelationshipNames;
            }

            try
            {
                string query = $"MATCH (r:{_tenantId}_RelTableNames) WHERE r.relationship_name IS NOT NULL AND r.orig_class IS NOT NULL AND r.dest_class IS NOT NULL RETURN DISTINCT r.relationship_name AS relationship_name, r.orig_class AS orig_class, r.dest_class AS dest_class;";
                var requestDto = new KuzuDBMQueryRequestDto { Query = query };
                (string formattedString, object[] results) = await ExecuteQueryAsync(requestDto);

                List<RelationshipInfo> relationships = new();

                var lines = formattedString.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length; i += 3)
                {
                    if (i + 2 < lines.Length)
                    {
                        var relationshipInfo = new RelationshipInfo
                        {
                            RelationshipName = lines[i].Trim().Split(':')[1].Trim(),
                            OrigClass = lines[i + 1].Trim().Split(':')[1].Trim(),
                            DestClass = lines[i + 2].Trim().Split(':')[1].Trim()
                        };
                        relationships.Add(relationshipInfo);
                    }
                    else
                    {
                        Console.WriteLine($"Unexpected line format at index {i}: {lines[i]}");
                    }
                }

                _cachedRelationshipNames = relationships;
                return relationships;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving unique relationship names: {e.Message}");
                return new List<RelationshipInfo>();
            }
        }


        /// <summary>
        /// Retrieves nodes and their relationships within a specified depth from a given node ID in a specified table.
        /// </summary>
        /// <param name="tableName">The name of the node label/table to match (e.g., "Musician", "Band").</param>
        /// <param name="id">The ID of the node from which to start the search.</param>
        /// <param name="depth">The maximum number of hops (depth) to search for connected nodes.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the query as a string.</returns>
        public async Task<(object[]? results, string formattedString)> GetNodesWithinDepthByIdAsync(KuzuDBMNodesWithinDepthRequestDto request)
        {
            Console.WriteLine("Getting nodes withing lock.");
            await _connectionLock.WaitAsync();
            Console.WriteLine("Nodes Within Lock Acquired");

            try
            {
                string query = $@"
                MATCH (a:{_tenantId}_{request.TableName}) WHERE a.record_id = {request.Id}
                MATCH (a)-[r*1..{request.Depth}]-(b)
                RETURN
                    a AS Original_Node,
                    r AS RECURSIVE_RELATIONSHIP,
                    b AS Related_Node;";

                var requestDto = new KuzuDBMQueryRequestDto { Query = query };
                (string formattedString, object[] results) = await ExecuteQueryAsync(requestDto, false);

                if (results != null)
                {
                    if (request.Depth >= 2)
                    {
                        return (null, formattedString);
                    }
                    else
                    {
                        return (results, "NULL");
                    }
                }
                else
                {
                    Console.WriteLine($"There are no relationships for the {request.TableName} with id {request.Id} that are in your project.");
                    return (null, "NULL");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving nodes within depth: {e.Message}");
                throw;
            }
            finally
            {
                _connectionLock.Release();
                Console.WriteLine("Nodes within lock released.");
            }
        }


        /// <summary>
        /// Dynamically loads both records and edges into the Kuzu database for a specified project.
        /// </summary>
        /// <param name="project_id">The project ID to filter the records and edges being loaded.</param>
        /// <returns>Returns true if there was an error during loading, otherwise false.</returns>
        public async Task<bool> LoadDataAsync(int project_id)
        {
            bool hasError;

            hasError = await LoadRecordsAsync(project_id);
            if (hasError)
            {
                return hasError;
            }

            hasError = await LoadEdgesAsync(project_id);
            return hasError;
        }


        /// <summary>
        /// Dynamically loads records from the PostgreSQL database into the KuzuDB.
        /// </summary>
        /// <param name="project_id">The project ID to filter the records being loaded.</param>
        /// <returns>Returns true if there was an error during loading, otherwise false.</returns>
        private async Task<bool> LoadRecordsAsync(int project_id)
        {
            bool hasError = false;

            try
            {
                await PerformNonQueryAsync($@"
                    COPY {_tenantId}_Entity FROM (LOAD FROM historical_records_c
                        WHERE class_name IS NULL AND project_id = {project_id}
                        RETURN
                            id,
                            uri,
                            record_id,
                            properties,
                            data_source_id,
                            original_id,
                            class_id,
                            name,
                            mapping_id,
                            class_name,
                            project_id,
                            project_name,
                            data_source_name,
                            tags,
                            created_by,
                            created_at,
                            modified_by,
                            modified_at,
                            archived_at,
                            last_updated_at
                    );"
                );

                var classNames = await GetUniqueClassNamesAsync();

                foreach (var className in classNames)
                {
                    string copyCommand = $@"
                        COPY {_tenantId}_{CapitalizeFirstLetter(className)} FROM (
                            LOAD FROM historical_records_c
                            WHERE class_name = '{className}' AND project_id = {project_id}
                            RETURN
                                id,
                                uri,
                                record_id,
                                properties,
                                data_source_id,
                                original_id,
                                class_id,
                                name,
                                mapping_id,
                                class_name,
                                project_id,
                                project_name,
                                data_source_name,
                                tags,
                                created_by,
                                created_at,
                                modified_by,
                                modified_at,
                                archived_at,
                                last_updated_at
                    );";

                    await PerformNonQueryAsync(copyCommand);
                }

                Console.WriteLine("Records loaded into KuzuDB.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while loading records: {e.Message}");
                hasError = true;
            }

            return hasError;
        }



        /// <summary>
        /// Dynamically loads edges from the edges_c view into the KuzuDB.
        /// </summary>
        /// <param name="project_id">The project ID to filter the edges being loaded.</param>
        /// <returns>Returns true if there was an error during loading, otherwise false.</returns>
        private async Task<bool> LoadEdgesAsync(int project_id)
        {
            bool hasError = false;
            try
            {
                await LoadRelatesToDataAsync(project_id);
                var relationships = await GetUniqueRelationshipNamesAsync();

                foreach (var relationship in relationships)
                {
                    string copyCommand = $@"
                            COPY {_tenantId}_{relationship.RelationshipName.ToUpper()} FROM (
                                LOAD FROM edges_c
                                WHERE relationship_name =~ '(?i){relationship.RelationshipName}' AND project_id = {project_id} AND orig_class =~ '(?i){relationship.OrigClass}' AND dest_class =~ '(?i){relationship.DestClass}'
                                RETURN origin_id AS FROM, destination_id AS TO, '{relationship.RelationshipName}' AS relationship_name
                            ) (from='{_tenantId}_{CapitalizeFirstLetter(relationship.OrigClass)}', to='{_tenantId}_{CapitalizeFirstLetter(relationship.DestClass)}');";
                    await PerformNonQueryAsync(copyCommand);
                }

                Console.WriteLine("Edges loaded into KuzuDB.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while loading edges: {e.Message}");
                hasError = true;
            }

            return hasError;
        }


        private async Task SyncRecordsAsync(int projectId)
        {
            try
            {
                List<KuzuDBMResponseDto>? currentRecords = await GetRecordsFromPostgresAsync(projectId);
                if (currentRecords == null || currentRecords.Count == 0)
                {
                    Console.WriteLine("No current records found in the PostgreSQL database.");
                    return;
                }

                List<KuzuDBMResponseDto?>? kuzuRecords = await GetRecordsFromKuzu(projectId);
                if (kuzuRecords == null || kuzuRecords.Count == 0)
                {
                    Console.WriteLine("No records found in the Kuzu database.");
                    return;
                }


                foreach (var currentRecord in currentRecords)
                {
                    var existingRecord = kuzuRecords.FirstOrDefault(r => r?.RecordId == currentRecord.RecordId);

                    if (existingRecord != null)
                    {
                        if (currentRecord.LastUpdatedAt > existingRecord.LastUpdatedAt)
                        {
                            await UpdateRecordInKuzuAsync(currentRecord);
                        }
                    }
                    else
                    {
                        await AddRecordToKuzuAsync(currentRecord);
                    }
                }

                foreach (var kuzuRecord in kuzuRecords)
                {
                    if (!currentRecords.Any(r => r.RecordId == kuzuRecord?.RecordId))
                    {
                        await RemoveRecordFromKuzuAsync(kuzuRecord?.RecordId);
                    }
                }

                Console.WriteLine("Synchronization of records completed successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred during synchronization: {e.Message}");
            }
        }


        private async Task<List<KuzuDBMResponseDto?>?> GetRecordsFromKuzu(int projectId)
        {
            try
            {
                var request = new KuzuDBMQueryRequestDto
                {
                    Query = $"MATCH (n) WHERE n.project_id = {projectId} RETURN n;"
                };

                (string formattedResponse, object[] records) = await ExecuteQueryAsync(request);

                if (records == null || records.Length == 0)
                {
                    return [];
                }

                List<KuzuDBMResponseDto?>? responseList = [];

                foreach (var record in records)
                {
                    if (record is KuzuDBMResponseDto kuzuRecord)
                    {
                        if (kuzuRecord?.Label?.Contains("ProcessedProjectIds") != true)
                        {
                            responseList.Add(kuzuRecord ?? null);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Record is not of type KuzuDBMResponseDto.");
                    }
                }

                return responseList;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving records from Kuzu: {e.Message}");
                return null;
            }
        }


        public async Task<bool> UpdateRelationshipInEdgeAsync(string originTableName, long originNodeId, string relatedTableName, long relatedNodeId, string newRelationshipType, string oldRelationshipType)
        {
            Console.WriteLine("Getting update relationship edges lock");
            await _connectionLock.WaitAsync();
            Console.WriteLine("Acquired update relationship edges lock.");

            try
            {
                string createRelTableQuery = $@"
                    CREATE REL TABLE IF NOT EXISTS {_tenantId}_{newRelationshipType} (
                        FROM {_tenantId}_{originTableName}
                        TO {_tenantId}_{relatedTableName},
                        relationship_name STRING
                    );";
                await PerformNonQueryAsync(createRelTableQuery);
                Console.WriteLine($"Ensured the REL TABLE {newRelationshipType} exists.");

                try
                {
                    string alterRelTableQuery = $@"
                        ALTER TABLE {_tenantId}_{newRelationshipType} ADD IF NOT EXISTS FROM {_tenantId}_{originTableName} TO {_tenantId}_{relatedTableName};";
                    Console.WriteLine($"Executing query: {alterRelTableQuery}");
                    await PerformNonQueryAsync(alterRelTableQuery);
                    Console.WriteLine($"Ensured the REL TABLE {newRelationshipType} includes FROM {originTableName} TO {relatedTableName}.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to alter relationship table: {e.Message}. Skipping this step.");
                }

                string updateQuery = $@"
                    MATCH (a:{_tenantId}_{originTableName}), (b:{_tenantId}_{relatedTableName})
                    WHERE a.id = {originNodeId} AND b.id = {relatedNodeId}
                    CREATE (a)-[:{_tenantId}_{newRelationshipType}]->(b);";
                await PerformNonQueryAsync(updateQuery);

                Console.WriteLine($"Updated relationship from {originTableName} with ID {originNodeId} to {relatedTableName} with ID {relatedNodeId} as {newRelationshipType}.");

                string deleteQuery = $@"
                    MATCH (a:{_tenantId}_{originTableName})-[r:{_tenantId}_{oldRelationshipType}]->(b:{_tenantId}_{relatedTableName})
                    WHERE a.id = {originNodeId} AND b.id = {relatedNodeId}
                    DELETE r;";
                await PerformNonQueryAsync(updateQuery);

                Console.WriteLine($"Deleted the relationship from the {oldRelationshipType} table.");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while updating the relationship: {e.Message}");
                return false;
            }
            finally
            {
                _connectionLock.Release();
                Console.WriteLine("Update relationship in edges lock released.");
            }
        }



        /// <summary>
        /// Updates default names in the RELATES_TO table and Entity table.
        /// Sets relationship_name to 'relates_to' where it is NULL and class_name to 'entity' where it is NULL.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdateDefaultNamesAsync()
        {
            try
            {
                await PerformNonQueryAsync($"MATCH ()-[r:{_tenantId}_RELATES_TO]-() WHERE r.relationship_name IS NULL SET r.relationship_name = 'relates_to';");

                await PerformNonQueryAsync($"MATCH (e:{_tenantId}_Entity) WHERE e.class_name IS NULL SET e.class_name = 'entity';");

                Console.WriteLine("Default names updated successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while updating default names: {e.Message}");
            }
        }


        /// <summary>
        /// Loads data into the RELATES_TO table based on unique relationships.
        /// </summary>
        /// <param name="project_id">The project ID to filter the records being loaded.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadRelatesToDataAsync(int project_id)
        {
            try
            {
                var relationships = await GetUniqueRelationshipNamesAsync();

                var groupedRelationships = relationships.GroupBy(r => r.OrigClass).ToList();

                foreach (var group in groupedRelationships)
                {
                    foreach (var relationship in group)
                    {
                        string copyCommand = $@"
                            COPY {_tenantId}_RELATES_TO FROM (
                                LOAD FROM edges_c
                                WHERE relationship_name IS NULL AND project_id = {project_id} AND orig_class = '{relationship.OrigClass.ToLower()}' AND dest_class = '{relationship.DestClass.ToLower()}'
                                RETURN origin_id AS FROM, destination_id AS TO, relationship_name
                            ) (from='{_tenantId}_{CapitalizeFirstLetter(relationship.OrigClass)}', to='{_tenantId}_{CapitalizeFirstLetter(relationship.DestClass)}');";

                        await PerformNonQueryAsync(copyCommand);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while loading data into the RELATES_TO table: {e.Message}");
            }
        }


        /// <summary>
        /// Closes the Kuzu connection.
        /// </summary>
        /// <returns>Returns false if there is no connection or if an error occurred and true if not.</returns>
        public async Task<bool> CloseAsync()
        {
            Console.WriteLine("Getting Close lock");
            await _connectionLock.WaitAsync();
            Console.WriteLine("Close lock acquired");

            try
            {
                if (_conn != null && _db != null)
                {
                    _conn.Dispose();
                    _conn = null;

                    _db.Dispose();
                    _db = null;

                    Console.WriteLine("Closed the Kuzu connection and database.");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while closing the Kuzu connection: {e.Message}");
                return false;
            }
            finally
            {
                _connectionLock.Release();
                Console.WriteLine("Close lock released");
            }
        }


        /// <summary>
        /// Executes a query on the Kuzu database and returns the formatted result.
        /// </summary>
        /// <param name="query">The Cypher query string to be executed.</param>
        /// <returns>A string containing the formatted results of the query.</returns>
        public async Task<(string formattedString, object[] results)> ExecuteQueryAsync(KuzuDBMQueryRequestDto request, bool DoAddTenantIdToQuery = true)
        {
            kuzu_query_result result = new();

            if (_conn == null)
            {
                await ConnectAsync();
            }

            try
            {
                if (DoAddTenantIdToQuery)
                {
                    request.Query = AddTenantIdToQuery(request.Query);
                }

                var state = await Task.Run(() => kuzu_connection_query(_conn, request.Query, result));

                if (state == kuzu_state.KuzuError)
                {
                    string errorMessage = kuzu_query_result_get_error_message(result);
                    Console.WriteLine($"Error executing query: {request.Query}");
                    Console.WriteLine($"Error Message: {errorMessage}");
                    throw new InvalidOperationException($"Error: {errorMessage}");
                }

                (string formattedString, object[] results) = await FormatQueryResultAsync(result);
                return (formattedString, results);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while executing the query: {e.Message}");
                throw;
            }
        }


        /// <summary>
        /// Prepends the tenantId to all labels in the query string.
        /// </summary>
        /// <param name="query">The original query string.</param>
        /// <returns>The modified query string with tenantId prepended to labels.</returns>
        private string AddTenantIdToQuery(string query)
        {

            if (_cachedClassNames != null)
            {
                foreach (var className in _cachedClassNames)
                {
                    query = query.Replace($":{CapitalizeFirstLetter(className)}", $":{_tenantId}_{CapitalizeFirstLetter(className)}");
                }
            }

            if (_cachedRelationshipNames != null)
            {
                foreach (var relationship in _cachedRelationshipNames)
                {
                    query = query.Replace($":{relationship.RelationshipName}", $":{_tenantId}_{relationship.RelationshipName}");
                }
            }


            return query;
        }


        /// <summary>
        /// Executes a non-query command against the Kuzu database.
        /// </summary>
        /// <param name="query">The Cypher query string to be executed.</param>
        private async Task PerformNonQueryAsync(string query)
        {
            try
            {
                using kuzu_query_result result = new();
                var state = await Task.Run(() => kuzu_connection_query(_conn, query, result));

                if (state == kuzu_state.KuzuError)
                {
                    string errorMessage = kuzu_query_result_get_error_message(result);
                    throw new InvalidOperationException($"Failed to execute query: {query}. Error: {errorMessage}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while performing the query: {e.Message}");
                throw;
            }
        }


        private static string ExtractLabel(string input)
        {
            string pattern = @"{_LABEL:\s*(?<label>[^\s}]+)";

            Regex regex = new(pattern);

            Match match = regex.Match(input);

            if (match.Success)
            {
                return match.Groups["label"].Value;
            }

            return "Null";
        }


        /// <summary>
        /// Formats the Kuzu query result into a readable string.
        /// </summary>
        /// <param name="result">The result object containing the query results.</param>
        /// <returns>A string representation of the formatted query results.</returns>
        private async Task<(string formattedString, object[] results)> FormatQueryResultAsync(kuzu_query_result result)
        {
            ulong numColumns = kuzu_query_result_get_num_columns(result);
            StringBuilder sb = new();

            List<object> results = [];

            while (await Task.Run(() => kuzu_query_result_has_next(result)))
            {
                kuzu_flat_tuple tuple = new();
                await Task.Run(() => kuzu_query_result_get_next(result, tuple));

                if (numColumns == 1)
                {
                    kuzu_value singleValue = new();
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 0, singleValue));

                    kuzu_logical_type singleValueType = new();
                    await Task.Run(() => kuzu_value_get_data_type(singleValue, singleValueType));

                    var dataTypeId = await Task.Run(() => kuzu_data_type_get_id(singleValueType));

                    if (dataTypeId == kuzu_data_type_id.KUZU_NODE)
                    {
                        KuzuDBMResponseDto? record = await GetRecordAsync(singleValue, singleValueType);
                        if (record != null)
                        {
                            results.Add(record);
                            sb.AppendLine(record.ToString());
                        }
                    }
                    else
                    {
                        sb.AppendLine(await GetValueStringAsync(singleValue, singleValueType));
                    }
                }
                else
                {
                    kuzu_value originalNode = new();
                    kuzu_value relationship = new();
                    kuzu_value relatedNode = new();

                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 0, originalNode));
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 1, relationship));
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 2, relatedNode));

                    kuzu_logical_type originalNodeDataType = new();
                    kuzu_logical_type relationshipDataType = new();
                    kuzu_logical_type relatedNodeDataType = new();

                    await Task.Run(() => kuzu_value_get_data_type(originalNode, originalNodeDataType));
                    await Task.Run(() => kuzu_value_get_data_type(relationship, relationshipDataType));
                    await Task.Run(() => kuzu_value_get_data_type(relatedNode, relatedNodeDataType));

                    if (!(kuzu_data_type_get_id(originalNodeDataType) == kuzu_data_type_id.KUZU_STRING && kuzu_data_type_get_id(relatedNodeDataType) == kuzu_data_type_id.KUZU_STRING && kuzu_data_type_get_id(relationshipDataType) == kuzu_data_type_id.KUZU_STRING))
                    {
                        string originalNodeColumnName = string.Empty;
                        string relatedNodeColumnName = string.Empty;
                        string relationshipColumnName = string.Empty;

                        await Task.Run(() => kuzu_query_result_get_column_name(result, 0, out originalNodeColumnName));
                        await Task.Run(() => kuzu_query_result_get_column_name(result, 1, out relationshipColumnName));
                        await Task.Run(() => kuzu_query_result_get_column_name(result, 2, out relatedNodeColumnName));

                        if (kuzu_data_type_get_id(originalNodeDataType) == kuzu_data_type_id.KUZU_NODE && kuzu_data_type_get_id(relatedNodeDataType) == kuzu_data_type_id.KUZU_NODE)
                        {
                            KuzuDBMResponseDto? originalRecordNode = await GetRecordAsync(originalNode, originalNodeDataType);
                            KuzuDBMResponseDto? relatedRecordNode = await GetRecordAsync(relatedNode, relatedNodeDataType);

                            if (originalRecordNode != null)
                            {
                                results.Add(originalRecordNode);
                            }

                            string relationshipString = await GetValueStringAsync(relationship, relationshipDataType);

                            KuzuRel relationshipNode = new()
                            {
                                RelationshipName = ExtractLabel(relationshipString),
                                SourceId = originalRecordNode?.InternalId,
                                DestId = relatedRecordNode?.InternalId
                            };

                            results.Add(relationshipNode);

                            if (relatedRecordNode != null)
                            {
                                results.Add(relatedRecordNode);
                            }

                            sb.AppendLine($"{originalNodeColumnName}: \n{await GetValueStringAsync(originalNode, originalNodeDataType)}");

                            sb.AppendLine($"{relationshipColumnName}: \n{await GetValueStringAsync(relationship, relationshipDataType)}");

                            sb.AppendLine($"{relatedNodeColumnName}: \n{await GetValueStringAsync(relatedNode, relatedNodeDataType)}");

                            sb.AppendLine($"-------------------------------------");
                        }
                        else
                        {
                            sb.AppendLine($"{originalNodeColumnName}: \n{await GetValueStringAsync(originalNode, originalNodeDataType)}");

                            sb.AppendLine($"{relationshipColumnName}: \n{await GetValueStringAsync(relationship, relationshipDataType)}");

                            sb.AppendLine($"{relatedNodeColumnName}: \n{await GetValueStringAsync(relatedNode, relatedNodeDataType)}");

                            sb.AppendLine($"-------------------------------------");
                        }


                        for (ulong i = 2; i < numColumns; i++)
                        {
                            kuzu_value columnValue = new();
                            await Task.Run(() => kuzu_flat_tuple_get_value(tuple, i, columnValue));
                            string columnName = string.Empty;
                            await Task.Run(() => kuzu_query_result_get_column_name(result, i, out columnName));
                            kuzu_logical_type columnDataType = new();
                            await Task.Run(() => kuzu_value_get_data_type(columnValue, columnDataType));
                            if (!(columnName == "Related_Node") && !(columnName == "Original_Node") && !(columnName == "RECURSIVE_RELATIONSHIP"))
                            {
                                sb.AppendLine($"{columnName}: {await GetValueStringAsync(columnValue, columnDataType)}");
                            }
                        }
                    }
                    for (ulong i = 0; i < numColumns; i++)
                    {
                        kuzu_value columnValue = new();
                        await Task.Run(() => kuzu_flat_tuple_get_value(tuple, i, columnValue));
                        string columnName = string.Empty;
                        await Task.Run(() => kuzu_query_result_get_column_name(result, i, out columnName));
                        kuzu_logical_type columnDataType = new();
                        await Task.Run(() => kuzu_value_get_data_type(columnValue, columnDataType));
                        if (!(columnName == "Related_Node") && !(columnName == "Original_Node") && !(columnName == "RECURSIVE_RELATIONSHIP"))
                        {
                            sb.AppendLine($"{columnName}: {await GetValueStringAsync(columnValue, columnDataType)}");
                        }
                    }
                    sb.AppendLine();
                }

                await Task.Run(() => kuzu_flat_tuple_destroy(tuple));
            }

            string formattedString = sb.ToString();
            return (formattedString, results.ToArray());
        }

        private async Task<KuzuDBMResponseDto?> GetRecordAsync(kuzu_value value, kuzu_logical_type dataType)
        {
            try
            {
                if (await Task.Run(() => kuzu_value_is_null(value)))
                {
                    Console.WriteLine("The Record Kuzu value is null");
                    return null;
                }

                var dataTypeId = await Task.Run(() => kuzu_data_type_get_id(dataType));
                KuzuDBMResponseDto recordInfo = new();

                if (dataTypeId == kuzu_data_type_id.KUZU_NODE)
                {
                    return await Task.Run(async () =>
                    {
                        kuzu_internal_id_t internalNodeId = new();
                        kuzu_value idValue = new();

                        kuzu_node_val_get_id_val(value, idValue);
                        kuzu_value_get_internal_id(idValue, internalNodeId);
                        recordInfo.InternalId = internalNodeId.ToString();

                        kuzu_value labelValue = new();
                        kuzu_node_val_get_label_val(value, labelValue);
                        kuzu_value_get_string(labelValue, out string label);
                        recordInfo.Label = label;

                        kuzu_node_val_get_property_size(value, out ulong propertyCount);

                        for (ulong i = 0; i < propertyCount; i++)
                        {
                            kuzu_value propertyValue = new();
                            kuzu_node_val_get_property_value_at(value, i, propertyValue);

                            kuzu_node_val_get_property_name_at(value, i, out string propertyName);
                            kuzu_logical_type propertyDataType = new();
                            kuzu_value_get_data_type(propertyValue, propertyDataType);

                            string propertyValueString = await GetValueStringAsync(propertyValue, propertyDataType);

                            // Console.WriteLine(" Property value string: " + propertyValueString);
                            // Console.WriteLine(" Property name: " + propertyName);

                            switch (propertyName)
                            {
                                case "created_at":
                                    recordInfo.CreatedAt = DateTime.TryParse(propertyValueString, out DateTime createdAt) ? createdAt : null;
                                    break;
                                case "modified_at":
                                    recordInfo.ModifiedAt = DateTime.TryParse(propertyValueString, out DateTime modifiedAt) ? modifiedAt : null;
                                    break;
                                case "tags":
                                    recordInfo.Tags = propertyValueString;
                                    break;
                                case "created_by":
                                    recordInfo.CreatedBy = propertyValueString;
                                    break;
                                case "class_name":
                                    recordInfo.ClassName = propertyValueString;
                                    break;
                                case "last_updated_at":
                                    recordInfo.LastUpdatedAt = DateTime.TryParse(propertyValueString, out DateTime lastUpdated) ? lastUpdated : null;
                                    break;
                                case "archived_at":
                                    recordInfo.ArchivedAt = DateTime.TryParse(propertyValueString, out DateTime archivedAt) ? archivedAt : null;
                                    break;
                                case "id":
                                    recordInfo.Id = long.TryParse(propertyValueString, out long id) ? id : null;
                                    break;
                                case "uri":
                                    recordInfo.Uri = propertyValueString;
                                    break;
                                case "record_id":
                                    recordInfo.RecordId = long.TryParse(propertyValueString, out long recordId) ? recordId : null;
                                    break;
                                case "properties":
                                    recordInfo.Properties = propertyValueString;
                                    break;
                                case "data_source_id":
                                    recordInfo.DataSourceId = long.TryParse(propertyValueString, out long dataSourceId) ? dataSourceId : null;
                                    break;
                                case "original_id":
                                    recordInfo.OriginalId = propertyValueString;
                                    break;
                                case "class_id":
                                    recordInfo.ClassId = long.TryParse(propertyValueString, out long classId) ? classId : null;
                                    break;
                                case "name":
                                    recordInfo.Name = propertyValueString;
                                    break;
                                case "project_name":
                                    recordInfo.ProjectName = propertyValueString;
                                    break;
                                case "modified_by":
                                    recordInfo.ModifiedBy = propertyValueString;
                                    break;
                                case "project_id":
                                    recordInfo.ProjectId = long.TryParse(propertyValueString, out long projectId) ? projectId : null;
                                    break;
                                default:
                                    // Console.WriteLine($"Unhandled property: {propertyName}");
                                    break;
                            }
                        }

                        return recordInfo;
                    });
                }
                else
                {
                    Console.WriteLine("The Record Value is Not a Kuzu_node: " + dataTypeId);
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while converting value to RecordInfo: {e.Message}");
                return null;
            }
        }


        /// <summary>
        /// Converts a Kuzu value into its string representation based on its logical type.
        /// </summary>
        /// <param name="value">The Kuzu value to be converted.</param>
        /// <param name="dataType">The logical type of the Kuzu value.</param>
        /// <returns>A string representation of the value. If the value is null, returns "NULL".</returns>
        private async Task<string> GetValueStringAsync(kuzu_value value, kuzu_logical_type dataType)
        {
            try
            {
                if (await Task.Run(() => kuzu_value_is_null(value)))
                {
                    return "NULL";
                }

                var dataTypeId = await Task.Run(() => kuzu_data_type_get_id(dataType));

                switch (dataTypeId)
                {
                    case kuzu_data_type_id.KUZU_STRING:
                        return await Task.Run(() =>
                        {
                            kuzu_value_get_string(value, out string result);
                            return $"{result}";
                        });

                    case kuzu_data_type_id.KUZU_INT64:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_int64(value, out long result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_INT32:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_int32(value, out int result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_FLOAT:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_float(value, out float result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_DOUBLE:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_double(value, out double result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_BOOL:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_bool(value, out bool result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_NODE:
                        return await Task.Run(async () =>
                        {
                            StringBuilder sb = new();

                            kuzu_internal_id_t internalNodeId = new();
                            kuzu_value idValue = new();

                            kuzu_node_val_get_id_val(value, idValue);
                            kuzu_value_get_internal_id(idValue, internalNodeId);

                            sb.AppendLine($" _ID: {internalNodeId.ToString()}");

                            kuzu_value labelValue = new();
                            kuzu_node_val_get_label_val(value, labelValue);
                            kuzu_value_get_string(labelValue, out string label);

                            kuzu_node_val_get_property_size(value, out ulong propertyCount);

                            sb.AppendLine($" _LABEL: {label},");

                            for (ulong i = 0; i < propertyCount; i++)
                            {
                                kuzu_value propertyValue = new();
                                kuzu_node_val_get_property_value_at(value, i, propertyValue);

                                kuzu_node_val_get_property_name_at(value, i, out string propertyName);
                                kuzu_logical_type propertyDataType = new();
                                kuzu_value_get_data_type(propertyValue, propertyDataType);

                                sb.AppendLine($" {propertyName}: {await GetValueStringAsync(propertyValue, propertyDataType)},");
                            }
                            sb.AppendLine("}");
                            return sb.ToString();
                        });

                    case kuzu_data_type_id.KUZU_REL:
                        return await Task.Run(() =>
                        {
                            kuzu_internal_id_t internalSrcId = new();
                            kuzu_internal_id_t internalDstId = new();

                            kuzu_value srcIdValue = new();
                            kuzu_rel_val_get_src_id_val(value, srcIdValue);
                            kuzu_value_get_internal_id(srcIdValue, internalSrcId);

                            kuzu_value dstIdValue = new();
                            kuzu_rel_val_get_dst_id_val(value, dstIdValue);
                            kuzu_value_get_internal_id(dstIdValue, internalDstId);

                            kuzu_value labelRelValue = new();
                            kuzu_rel_val_get_label_val(value, labelRelValue);
                            kuzu_value_get_string(labelRelValue, out string relLabel);

                            relLabel = relLabel.Replace($"{_tenantId}_", "");

                            return $"({internalSrcId})-{{_LABEL: {relLabel}}}->({internalDstId})";
                        });

                    case kuzu_data_type_id.KUZU_RECURSIVE_REL:
                        return await Task.Run(async () =>
                        {
                            StringBuilder sbRecursive = new();
                            kuzu_value nodeList = new();

                            HashSet<string> printedRelationships = [];

                            kuzu_value relationshipList = new();
                            kuzu_value_get_recursive_rel_node_list(value, nodeList);
                            kuzu_value_get_recursive_rel_rel_list(value, relationshipList);

                            ulong nodeCount;
                            kuzu_value_get_list_size(nodeList, out nodeCount);

                            for (ulong i = 0; i < nodeCount; i++)
                            {
                                kuzu_value currentNode = new();
                                kuzu_value_get_list_element(nodeList, i, currentNode);
                                kuzu_logical_type nodeDataType = new();
                                await Task.Run(() => kuzu_value_get_data_type(currentNode, nodeDataType));

                                KuzuDBMResponseDto? recordInfo = await GetRecordAsync(currentNode, nodeDataType);

                                sbRecursive.AppendLine($"Node: \n{recordInfo?.ToString()}");
                            }

                            ulong relationCount;
                            kuzu_value_get_list_size(relationshipList, out relationCount);

                            for (ulong j = 0; j < relationCount; j++)
                            {
                                kuzu_value currentRelation = new();
                                kuzu_value_get_list_element(relationshipList, j, currentRelation);
                                kuzu_logical_type relationDataType = new();
                                await Task.Run(() => kuzu_value_get_data_type(currentRelation, relationDataType));

                                string relationshipString = $"{await GetValueStringAsync(currentRelation, relationDataType)}";

                                if (printedRelationships.Add(relationshipString))
                                {
                                    sbRecursive.AppendLine($"Relationship: {relationshipString}");
                                }
                            }

                            return sbRecursive.ToString();
                        });

                    case kuzu_data_type_id.KUZU_INT8:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_int8(value, out sbyte result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_INT16:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_int16(value, out short result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_UINT8:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_uint8(value, out byte result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_UINT16:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_uint16(value, out ushort result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_UINT32:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_uint32(value, out uint result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_UINT64:
                        return (await Task.Run(() =>
                        {
                            kuzu_value_get_uint64(value, out ulong result);
                            return result;
                        })).ToString();

                    case kuzu_data_type_id.KUZU_INT128:
                        return await Task.Run(() =>
                        {
                            kuzu_int128_t result = new();
                            kuzu_value_get_int128(value, result);
                            return result?.ToString() ?? "NULL";
                        });

                    case kuzu_data_type_id.KUZU_INTERNAL_ID:
                        return await Task.Run(() =>
                        {
                            kuzu_internal_id_t result = new();
                            kuzu_value_get_internal_id(value, result);
                            return result.ToString();
                        });

                    case kuzu_data_type_id.KUZU_DATE:
                        return await Task.Run(() =>
                        {
                            kuzu_date_t result = new();
                            kuzu_value_get_date(value, result);
                            return result?.ToString() ?? "NULL";
                        });

                    case kuzu_data_type_id.KUZU_TIMESTAMP:
                        return await Task.Run(() =>
                        {
                            kuzu_timestamp_t result = new();
                            tm timestamp = new();
                            kuzu_date_t date = new();
                            kuzu_timestamp_sec_t seconds = new();

                            kuzu_value_get_timestamp(value, result);
                            kuzu_timestamp_to_tm(result, timestamp);
                            kuzu_timestamp_sec_from_tm(timestamp, seconds);

                            return seconds?.ToDate() ?? "NULL";
                        });

                    case kuzu_data_type_id.KUZU_BLOB:
                        return await Task.Run(() =>
                        {
                            kuzu_value_get_blob(value, out byte[] result);
                            return $"<blob size: {result.Length}>";
                        });

                    case kuzu_data_type_id.KUZU_UUID:
                        return await Task.Run(() =>
                        {
                            kuzu_value_get_uuid(value, out string result);
                            return result;
                        });

                    default:
                        return $"Unsupported data type: {dataTypeId}";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while converting value to string: {e.Message}");
                return $"Error: {e.Message}";
            }
        }
    }


    /// <summary>
    /// Represents information about a relationship between two classes.
    /// </summary>
    public class RelationshipInfo
    {
        public required string RelationshipName { get; set; }

        public required string OrigClass { get; set; }

        public required string DestClass { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that contains the relationship name, originating class, and destination class.</returns>
        public override string ToString()
        {
            return $"relationship_name: {RelationshipName}, orig_class: {OrigClass}, dest_class: {DestClass}";
        }
    }

    public class KuzuRel
    {
        public string? RelationshipName { get; set; }
        public string? SourceId { get; set; }
        public string? DestId { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that contains the relationship name, source ID, and destination ID.</returns>
        public override string ToString()
        {
            return $"({SourceId})-{{_LABEL: {RelationshipName}}}->({DestId})";
        }
    }
    
    public class EdgeDto
    {
        public string? OriginClass { get; set; }
        public int? OriginId { get; set; }
        public string? DestinationClass { get; set; }
        public int? DestinationId { get; set; }
        public string? RelationshipName { get; set; }
        public int? ProjectId { get; set; }
        public int? EdgeId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }

}