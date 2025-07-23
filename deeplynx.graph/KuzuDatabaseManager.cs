using System;
using static kuzunet;
using System.Text;
using System.Data;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Threading.Tasks;

namespace deeplynx.graph
{
    public class KuzuDatabaseManager : IKuzuDatabaseManager
    {
        private readonly string _kuzuDbPath = "../deeplynx.graph/kuzu_db";
        private kuzu_database? _db;
        private kuzu_connection? _conn;
        private bool _isDatabaseInitialized = false;
        private readonly string _pgParams;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);


        /// <summary>
        /// Initializes a new instance of the KuzuDatabaseManager class.
        /// </summary>
        public KuzuDatabaseManager(IConfiguration configuration)
        {
            _db = new kuzu_database();
            _conn = new kuzu_connection();

            // Transform the connection string
            string originalConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");
            _pgParams = TransformConnectionString(originalConnectionString);
        }


        /// <summary>
        /// Transforms a connection string from the format "User ID=...;Password=...;Database=...;Server=...;Port=..." 
        /// to the format "dbname=... user=... host=... password=... port=...".
        /// </summary>
        /// <param name="input">The original connection string to transform.</param>
        /// <returns>The transformed connection string.</returns>
        private string TransformConnectionString(string input)
        {
            var keyValuePairs = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var dictionary = new Dictionary<string, string>();

            foreach (var pair in keyValuePairs)
            {
                var parts = pair.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    dictionary[parts[0].Trim()] = parts[1].Trim();
                }
            }

            // Build the transformed connection string
            var transformed = $"dbname={dictionary["Database"]} " +
                              $"user={dictionary["User ID"]} " +
                              $"host={dictionary["Server"]} " +
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
            if (_conn != null)
            {
                Console.WriteLine("Already connected to Kuzu database.");
                return true;
            }
            
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


        /// <summary>
        /// Installs the necessary PostgreSQL extensions for KuzuDB.
        /// </summary>
        /// <param name="pgParams">The connection parameters for the PostgreSQL database.</param>
        /// <returns>Returns false if an error occurred and true if not.</returns>
        public async Task<bool> InstallPostgresExtensionsAsync()
        {
            Console.WriteLine("Installing Postgres and Json extenstions.");
            try
            {
                if (_conn == null)
                {
                    await ConnectAsync();
                }

                await Task.Run(() => PerformNonQueryAsync("INSTALL postgres;"));
                await Task.Run(() => PerformNonQueryAsync("LOAD EXTENSION postgres;"));
                await Task.Run(() => PerformNonQueryAsync("INSTALL json;"));
                await Task.Run(() => PerformNonQueryAsync("LOAD EXTENSION json;"));

                Console.WriteLine("Installed Postgres and Json extensions.");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred when installing the Postgres and Json extensions: {e}");
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

                await InstallPostgresExtensionsAsync();

                string attachCommand = $"ATTACH '{_pgParams}' AS test (dbtype postgres, skip_unsupported_table = TRUE, schema = 'deeplynx');";
                await PerformNonQueryAsync(attachCommand);

                await PerformNonQueryAsync("CREATE NODE TABLE IF NOT EXISTS ProcessedProjectIds (project_id INT64 PRIMARY KEY);");

                bool projectIdExists = await CheckIfProjectIdExistsAsync(project_id);

                if (projectIdExists)
                {
                    Console.WriteLine($"Project ID {project_id} has already been processed. Skipping data load.");
                    return true;
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
                    return true;
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
                var query = $"MATCH (p:ProcessedProjectIds) WHERE p.project_id = {projectId} RETURN COUNT(p) > 0;";
                var requestDto = new KuzuDBMQueryRequestDto { Query = query };
                var result = await ExecuteQueryAsync(requestDto);

                return result.Contains("True");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while checking for project ID existence: {e.Message}");
                return false;
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
                var query = $"CREATE (p:ProcessedProjectIds {{project_id: {projectId}}});";
                await PerformNonQueryAsync(query);
                Console.WriteLine($"Project ID {projectId} inserted into ProcessedProjectIds.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while inserting project ID: {e.Message}");
            }
        }


        /// <summary>
        /// Dynamically sets up the necessary tables in the Kuzu database.
        /// </summary>
        private async Task SetupKuzuTablesAsync()
        {
            try
            {

                await PerformNonQueryAsync("CREATE NODE TABLE IF NOT EXISTS TableNames (id INT64, class_name STRING, PRIMARY KEY(id));");
                await PerformNonQueryAsync("COPY TableNames FROM (LOAD FROM historical_records RETURN id, class_name);");

                await PerformNonQueryAsync("CREATE NODE TABLE IF NOT EXISTS RelTableNames (id INT64, relationship_name STRING, orig_class STRING, dest_class STRING, PRIMARY KEY(id));");
                await PerformNonQueryAsync("COPY RelTableNames FROM (LOAD FROM edges_c RETURN DISTINCT id, relationship_name, orig_class, dest_class);");

                var classNames = await GetUniqueClassNamesAsync();

                foreach (var className in classNames)
                {
                    string createNodeTableQuery = $@"
                        CREATE NODE TABLE {CapitalizeFirstLetter(className)} (
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
                            project_name INT64,
                            data_source_name INT64,
                            tags STRING,
                            created_by STRING,
                            created_at TIMESTAMP,
                            modified_by STRING,
                            modified_at TIMESTAMP,
                            archived_at TIMESTAMP,
                            PRIMARY KEY (id)
                        );";

                    await PerformNonQueryAsync(createNodeTableQuery);
                }

                var relationships = await GetUniqueRelationshipNamesAsync();
                var relationshipGroups = relationships.GroupBy(r => r.RelationshipName).ToList();

                foreach (var group in relationshipGroups)
                {
                    var relationshipName = group.Key.ToUpper();
                    var fromToClauses = group.Select(r => $"FROM {CapitalizeFirstLetter(r.OrigClass)} TO {CapitalizeFirstLetter(r.DestClass)}").ToList();
                    var fromToClause = string.Join(", ", fromToClauses);

                    string createRelTableQuery = $@"
                        CREATE REL TABLE IF NOT EXISTS {relationshipName} (
                            {fromToClause},
                            relationship_name STRING
                        );";

                    await PerformNonQueryAsync(createRelTableQuery);
                }

                await PerformNonQueryAsync(@"
                    CREATE NODE TABLE Entity (
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
                        project_name INT64,
                        data_source_name INT64,
                        tags STRING,
                        created_by STRING,
                        created_at TIMESTAMP,
                        modified_by STRING,
                        modified_at TIMESTAMP,
                        archived_at TIMESTAMP,
                        PRIMARY KEY (id)
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
                    string fromToClause = $"FROM {CapitalizeFirstLetter(relationship.OrigClass)} TO {CapitalizeFirstLetter(relationship.DestClass)}";
                    fromToClauses.Add(fromToClause);
                }

                string combinedFromToClauses = string.Join(", ", fromToClauses);

                string createRelTableQuery = $@"
                    CREATE REL TABLE IF NOT EXISTS RELATES_TO (
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
        private string CapitalizeFirstLetter(string input)
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
            try
            {
                string query = "MATCH (t:TableNames) WHERE t.class_name IS NOT NULL RETURN DISTINCT t.class_name;";
                var requestDto = new KuzuDBMQueryRequestDto { Query = query };
                string result = await ExecuteQueryAsync(requestDto);

                List<string> classNames = new List<string>();

                var lines = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (!string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        classNames.Add(trimmedLine);
                    }
                }

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
            try
            {
                string query = "MATCH (r:RelTableNames) WHERE r.relationship_name IS NOT NULL AND r.orig_class IS NOT NULL AND r.dest_class IS NOT NULL RETURN DISTINCT r.relationship_name AS relationship_name, r.orig_class AS orig_class, r.dest_class AS dest_class;";
                var requestDto = new KuzuDBMQueryRequestDto { Query = query };
                string result = await ExecuteQueryAsync(requestDto);

                List<RelationshipInfo> relationships = new List<RelationshipInfo>();

                var lines = result.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

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
        public async Task<string> GetNodesWithinDepthByIdAsync(KuzuDBMNodesWithinDepthRequestDto request)
        {
            Console.WriteLine("Getting nodes withing lock.");
            await _connectionLock.WaitAsync();
            Console.WriteLine("Nodes Within Lock Acquired");

            try
            {
                string query = $@"
                MATCH (a:{request.TableName}) WHERE a.id = {request.Id}
                MATCH (a)-[r*1..{request.Depth}]-(b)
                RETURN
                    a AS Original_Node,
                    r AS RECURSIVE_RELATIONSHIP,
                    b AS Related_Node;";

                var requestDto = new KuzuDBMQueryRequestDto { Query = query };
                var result = await ExecuteQueryAsync(requestDto);

                if (result.Contains("id"))
                {
                    return result;
                }
                else
                {
                    return $"There are no relationships for the {request.TableName} with id {request.Id} that are in project your project.";
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
                    COPY Entity FROM (LOAD FROM historical_records
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
                            archived_at
                    );"
                );

                var classNames = await GetUniqueClassNamesAsync();

                foreach (var className in classNames)
                {
                    string copyCommand = $@"
                        COPY {CapitalizeFirstLetter(className)} FROM (
                            LOAD FROM historical_records
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
                                archived_at
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
                            COPY {relationship.RelationshipName.ToUpper()} FROM (
                                LOAD FROM edges_c
                                WHERE relationship_name =~ '(?i){relationship.RelationshipName}' AND project_id = {project_id} AND orig_class =~ '(?i){relationship.OrigClass}' AND dest_class =~ '(?i){relationship.DestClass}'
                                RETURN origin_id AS FROM, destination_id AS TO, '{relationship.RelationshipName}' AS relationship_name
                            ) (from='{CapitalizeFirstLetter(relationship.OrigClass)}', to='{CapitalizeFirstLetter(relationship.DestClass)}');";
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


        public async Task<bool> UpdateRelationshipInEdgeAsync(string originTableName, long originNodeId, string relatedTableName, long relatedNodeId, string newRelationshipType, string oldRelationshipType)
        {
            Console.WriteLine("Getting update relationship edges lock");
            await _connectionLock.WaitAsync();
            Console.WriteLine("Acquired update relationship edges lock.");

            try
            {
                string createRelTableQuery = $@"
                    CREATE REL TABLE IF NOT EXISTS {newRelationshipType} (
                        FROM {originTableName}
                        TO {relatedTableName},
                        relationship_name STRING
                    );";
                await PerformNonQueryAsync(createRelTableQuery);
                Console.WriteLine($"Ensured the REL TABLE {newRelationshipType} exists.");

                try
                {
                    string alterRelTableQuery = $@"
                        ALTER TABLE {newRelationshipType} ADD IF NOT EXISTS FROM {originTableName} TO {relatedTableName};";
                    Console.WriteLine($"Executing query: {alterRelTableQuery}");
                    await PerformNonQueryAsync(alterRelTableQuery);
                    Console.WriteLine($"Ensured the REL TABLE {newRelationshipType} includes FROM {originTableName} TO {relatedTableName}.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to alter relationship table: {e.Message}. Skipping this step.");
                }


                string updateQuery = $@"
                    MATCH (a:{originTableName}), (b:{relatedTableName})
                    WHERE a.id = {originNodeId} AND b.id = {relatedNodeId}
                    CREATE (a)-[:{newRelationshipType}]->(b);";
                await PerformNonQueryAsync(updateQuery);

                Console.WriteLine($"Updated relationship from {originTableName} with ID {originNodeId} to {relatedTableName} with ID {relatedNodeId} as {newRelationshipType}.");

                string deleteQuery = $@"
                    MATCH (a:{originTableName})-[r:{oldRelationshipType}]->(b:{relatedTableName})
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
                await PerformNonQueryAsync("MATCH ()-[r:RELATES_TO]-() WHERE r.relationship_name IS NULL SET r.relationship_name = 'relates_to';");

                await PerformNonQueryAsync("MATCH (e:Entity) WHERE e.class_name IS NULL SET e.class_name = 'entity';");

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
                    COPY RELATES_TO FROM (
                        LOAD FROM edges_c
                        WHERE relationship_name IS NULL AND project_id = {project_id} AND orig_class = '{relationship.OrigClass.ToLower()}' AND dest_class = '{relationship.DestClass.ToLower()}'
                        RETURN origin_id AS FROM, destination_id AS TO, relationship_name
                    ) (from='{CapitalizeFirstLetter(relationship.OrigClass)}', to='{CapitalizeFirstLetter(relationship.DestClass)}');";

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
        public async Task<string> ExecuteQueryAsync(KuzuDBMQueryRequestDto request)
        {
            kuzu_query_result result = new kuzu_query_result();

            if (_conn == null)
            {
                await ConnectAsync();
            }

            try
            {
                var state = await Task.Run(() => kuzu_connection_query(_conn, request.Query, result));

                if (state == kuzu_state.KuzuError)
                {
                    string errorMessage = kuzu_query_result_get_error_message(result);
                    Console.WriteLine($"Error executing query: {request.Query}");
                    Console.WriteLine($"Error Message: {errorMessage}");
                    throw new InvalidOperationException($"Error: {errorMessage}");
                }

                string res = await FormatQueryResultAsync(result);

                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while executing the query: {e.Message}");
                throw;
            }
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

        /// <summary>
        /// Formats the Kuzu query result into a readable string.
        /// </summary>
        /// <param name="result">The result object containing the query results.</param>
        /// <returns>A string representation of the formatted query results.</returns>
        private async Task<string> FormatQueryResultAsync(kuzu_query_result result)
        {
            ulong numColumns = kuzu_query_result_get_num_columns(result);
            StringBuilder sb = new StringBuilder();

            while (await Task.Run(() => kuzu_query_result_has_next(result)))
            {
                kuzu_flat_tuple tuple = new kuzu_flat_tuple();
                await Task.Run(() => kuzu_query_result_get_next(result, tuple));

                if (numColumns == 1)
                {
                    kuzu_value singleValue = new kuzu_value();
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 0, singleValue));

                    kuzu_logical_type singleValueType = new kuzu_logical_type();
                    await Task.Run(() => kuzu_value_get_data_type(singleValue, singleValueType));

                    sb.AppendLine(await GetValueStringAsync(singleValue, singleValueType));
                }
                else
                {
                    kuzu_value originalNode = new kuzu_value();
                    kuzu_value relationship = new kuzu_value();
                    kuzu_value relatedNode = new kuzu_value();

                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 0, originalNode));
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 1, relationship));
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 2, relatedNode));

                    kuzu_logical_type originalNodeDataType = new kuzu_logical_type();
                    kuzu_logical_type relationshipDataType = new kuzu_logical_type();
                    kuzu_logical_type relatedNodeDataType = new kuzu_logical_type();

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

                        sb.AppendLine($"{originalNodeColumnName}: \n{await GetValueStringAsync(originalNode, originalNodeDataType)}");

                        sb.AppendLine($"{relationshipColumnName}: \n{await GetValueStringAsync(relationship, relationshipDataType)}");

                        sb.AppendLine($"{relatedNodeColumnName}: \n{await GetValueStringAsync(relatedNode, relatedNodeDataType)}");

                        sb.AppendLine($"-------------------------------------");

                        for (ulong i = 2; i < numColumns; i++)
                        {
                            kuzu_value columnValue = new kuzu_value();
                            await Task.Run(() => kuzu_flat_tuple_get_value(tuple, i, columnValue));
                            string columnName = string.Empty;
                            await Task.Run(() => kuzu_query_result_get_column_name(result, i, out columnName));
                            kuzu_logical_type columnDataType = new kuzu_logical_type();
                            await Task.Run(() => kuzu_value_get_data_type(columnValue, columnDataType));
                            if (!(columnName == "Related_Node") && !(columnName == "Original_Node") && !(columnName == "RECURSIVE_RELATIONSHIP"))
                            {
                                sb.AppendLine($"{columnName}: {await GetValueStringAsync(columnValue, columnDataType)}");
                            }
                        }
                    }
                    for (ulong i = 0; i < numColumns; i++)
                    {
                        kuzu_value columnValue = new kuzu_value();
                        await Task.Run(() => kuzu_flat_tuple_get_value(tuple, i, columnValue));
                        string columnName = string.Empty;
                        await Task.Run(() => kuzu_query_result_get_column_name(result, i, out columnName));
                        kuzu_logical_type columnDataType = new kuzu_logical_type();
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

            string res = sb.ToString();
            return res;
        }


        /// <summary>
        /// Converts a Kuzu value into its string representation based on its logical type.
        /// </summary>
        /// <param name="value">The Kuzu value to be converted.</param>
        /// <param name="dataType">The logical type of the Kuzu value.</param>
        /// <returns>A string representation of the value. If the value is null, returns "NULL".</returns>
        private static async Task<string> GetValueStringAsync(kuzu_value value, kuzu_logical_type dataType)
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
                            StringBuilder sb = new StringBuilder();

                            kuzu_internal_id_t internalNodeId = new kuzu_internal_id_t();
                            kuzu_value idValue = new kuzu_value();

                            kuzu_node_val_get_id_val(value, idValue);
                            kuzu_value_get_internal_id(idValue, internalNodeId);

                            sb.AppendLine($" _ID: {internalNodeId.ToString()}");

                            kuzu_value labelValue = new kuzu_value();
                            kuzu_node_val_get_label_val(value, labelValue);
                            kuzu_value_get_string(labelValue, out string label);

                            kuzu_node_val_get_property_size(value, out ulong propertyCount);

                            sb.AppendLine($" _LABEL: {label},");

                            for (ulong i = 0; i < propertyCount; i++)
                            {
                                kuzu_value propertyValue = new kuzu_value();
                                kuzu_node_val_get_property_value_at(value, i, propertyValue);

                                kuzu_node_val_get_property_name_at(value, i, out string propertyName);
                                kuzu_logical_type propertyDataType = new kuzu_logical_type();
                                kuzu_value_get_data_type(propertyValue, propertyDataType);

                                sb.AppendLine($" {propertyName}: {await GetValueStringAsync(propertyValue, propertyDataType)},");
                            }
                            sb.AppendLine("}");
                            return sb.ToString();
                        });

                    case kuzu_data_type_id.KUZU_REL:
                        return await Task.Run(() =>
                        {
                            kuzu_internal_id_t internalSrcId = new kuzu_internal_id_t();
                            kuzu_internal_id_t internalDstId = new kuzu_internal_id_t();

                            kuzu_value srcIdValue = new kuzu_value();
                            kuzu_rel_val_get_src_id_val(value, srcIdValue);
                            kuzu_value_get_internal_id(srcIdValue, internalSrcId);

                            kuzu_value dstIdValue = new kuzu_value();
                            kuzu_rel_val_get_dst_id_val(value, dstIdValue);
                            kuzu_value_get_internal_id(dstIdValue, internalDstId);

                            kuzu_value labelRelValue = new kuzu_value();
                            kuzu_rel_val_get_label_val(value, labelRelValue);
                            kuzu_value_get_string(labelRelValue, out string relLabel);

                            return $"({internalSrcId})-{{_LABEL: {relLabel}}}->({internalDstId})";
                        });

                    case kuzu_data_type_id.KUZU_RECURSIVE_REL:
                        return await Task.Run(async () =>
                        {
                            StringBuilder sbRecursive = new StringBuilder();
                            kuzu_value nodeList = new kuzu_value();

                            HashSet<string> printedRelationships = new HashSet<string>();

                            kuzu_value relationshipList = new kuzu_value();
                            kuzu_value_get_recursive_rel_node_list(value, nodeList);
                            kuzu_value_get_recursive_rel_rel_list(value, relationshipList);

                            ulong nodeCount;
                            kuzu_value_get_list_size(nodeList, out nodeCount);

                            for (ulong i = 0; i < nodeCount; i++)
                            {
                                kuzu_value currentNode = new kuzu_value();
                                kuzu_value_get_list_element(nodeList, i, currentNode);
                                kuzu_logical_type nodeDataType = new kuzu_logical_type();
                                await Task.Run(() => kuzu_value_get_data_type(currentNode, nodeDataType));
                                sbRecursive.AppendLine($"Node: \n{await GetValueStringAsync(currentNode, nodeDataType)}");
                            }

                            ulong relationCount;
                            kuzu_value_get_list_size(relationshipList, out relationCount);

                            for (ulong j = 0; j < relationCount; j++)
                            {
                                kuzu_value currentRelation = new kuzu_value();
                                kuzu_value_get_list_element(relationshipList, j, currentRelation);
                                kuzu_logical_type relationDataType = new kuzu_logical_type();
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
                            kuzu_int128_t result = new kuzu_int128_t();
                            kuzu_value_get_int128(value, result);
                            return result?.ToString() ?? "NULL";
                        });

                    case kuzu_data_type_id.KUZU_INTERNAL_ID:
                        return await Task.Run(() =>
                        {
                            kuzu_internal_id_t result = new kuzu_internal_id_t();
                            kuzu_value_get_internal_id(value, result);
                            return result.ToString();
                        });

                    case kuzu_data_type_id.KUZU_DATE:
                        return await Task.Run(() =>
                        {
                            kuzu_date_t result = new kuzu_date_t();
                            kuzu_value_get_date(value, result);
                            return result?.ToString() ?? "NULL";
                        });

                    case kuzu_data_type_id.KUZU_TIMESTAMP:
                        return await Task.Run(() =>
                        {
                            kuzu_timestamp_t result = new kuzu_timestamp_t();
                            tm timestamp = new tm();
                            kuzu_date_t date = new kuzu_date_t();
                            kuzu_timestamp_sec_t seconds = new kuzu_timestamp_sec_t();

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

}
