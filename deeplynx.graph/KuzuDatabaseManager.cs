using System;
using static kuzunet;
using System.Text;
using System.Data;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace deeplynx.graph
{
    public class KuzuDatabaseManager : IKuzuDatabaseManager
    {
        private readonly string _kuzuDbPath = "../deeplynx.graph/kuzu_db";
        private kuzu_database _db;
        private kuzu_connection? _conn;
        private bool _isDatabaseInitialized = false;
        private readonly string _pgParams;

        /// <summary>
        /// Initializes a new instance of the KuzuDatabaseManager class.
        /// </summary>
        public KuzuDatabaseManager(IConfiguration configuration)
        {
            _db = new kuzu_database();
            _conn = new kuzu_connection();
            _pgParams = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        }


        /// <summary>
        /// Connects to the Kuzu database asynchronously.
        /// Creates the database if it hasn't been initialized yet.
        /// </summary>
        /// <returns>A task representing the asynchronous connection operation.</returns>
        public async Task<bool> ConnectAsync()
        {
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
        }


        /// <summary>
        /// Installs the necessary PostgreSQL extensions for KuzuDB.
        /// </summary>
        /// <param name="pgParams">The connection parameters for the PostgreSQL database.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InstallPostgresExtensionsAsync()
        {
            if (_conn == null)
            {
                await ConnectAsync();
            }

            await Task.Run(() => PerformNonQueryAsync("INSTALL postgres;"));
            await Task.Run(() => PerformNonQueryAsync("LOAD EXTENSION postgres;"));
            await Task.Run(() => PerformNonQueryAsync("INSTALL json;"));
            await Task.Run(() => PerformNonQueryAsync("LOAD EXTENSION json;"));
        }


        /// <summary>
        /// Main export function that checks records and edges, assigns defaults, copies data, and indicates successful export.
        /// </summary>
        /// <param name="pgParams">The connection parameters for the PostgreSQL database.</param>
        /// <param name="project_id">The project identifier.</param>
        /// <returns>A task representing the asynchronous export operation.</returns>
        public async Task<bool> ExportDataAsync(int project_id)
        {

            if (_conn == null)
            {
                await ConnectAsync();
            }

            bool hasError = false;
            try
            {
                await InstallPostgresExtensionsAsync();

                string attachCommand = $"ATTACH '{_pgParams}' AS test (dbtype postgres, skip_unsupported_table = TRUE, schema = 'deeplynx');";
                await Task.Run(() => PerformNonQueryAsync(attachCommand));

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
                }

                if (!hasError)
                {
                    Console.WriteLine("Data export completed successfully.");
                    return true;
                }

                return hasError;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred during the export: {e.Message}");
                return false;
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
                            properties STRING,
                            data_source_id INT64,
                            original_id STRING,
                            name STRING,
                            class_name STRING,
                            project_id INT64,
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

                await PerformNonQueryAsync("CREATE NODE TABLE Entity (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");
                await CreateRelatesToTableAsync();

                Console.WriteLine("Tables setup successfully.");
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

                Console.WriteLine("RELATES_TO table setup successfully.");
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
                string query = "MATCH (r:RelTableNames) WHERE r.relationship_name IS NOT NULL RETURN DISTINCT r.relationship_name AS relationship_name, r.orig_class AS orig_class, r.dest_class AS dest_class;";
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
            try
            {
                string query = $@"
                MATCH (a:{request.TableName}) WHERE a.id = {request.Id}
                MATCH (a)-[r*1..{request.Depth}]-(b)
                RETURN
                    r AS RECURSIVE_RELATIONSHIP;";

                var requestDto = new KuzuDBMQueryRequestDto { Query = query };

                return await ExecuteQueryAsync(requestDto);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving nodes within depth: {e.Message}");
                throw;
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
                await PerformNonQueryAsync($"COPY Entity FROM (LOAD FROM historical_records WHERE class_name IS NULL AND project_id = {project_id} RETURN id, properties, data_source_id, original_id, name, class_name, project_id);");
                var classNames = await GetUniqueClassNamesAsync();

                foreach (var className in classNames)
                {
                    string copyCommand = $@"
                        COPY {CapitalizeFirstLetter(className)} FROM (
                            LOAD FROM historical_records
                            WHERE class_name = '{className}' AND project_id = {project_id}
                            RETURN id, properties, data_source_id, original_id, name, class_name, project_id
                        );";

                    await PerformNonQueryAsync(copyCommand);
                }

                Console.WriteLine("Records loaded successfully into KuzuDB.");
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
                                WHERE relationship_name = '{relationship.RelationshipName}' AND project_id = {project_id} AND orig_class = '{relationship.OrigClass.ToLower()}' AND dest_class = '{relationship.DestClass.ToLower()}'
                                RETURN origin_id AS FROM, destination_id AS TO, '{relationship.RelationshipName}' AS relationship_name
                            ) (from='{CapitalizeFirstLetter(relationship.OrigClass)}', to='{CapitalizeFirstLetter(relationship.DestClass)}');";

                    await PerformNonQueryAsync(copyCommand);
                }

                Console.WriteLine("Edges loaded successfully into KuzuDB.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while loading edges: {e.Message}");
                hasError = true;
            }

            return hasError;
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

                Console.WriteLine("Data loaded successfully into the RELATES_TO table.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while loading data into the RELATES_TO table: {e.Message}");
            }
        }


        /// <summary>
        /// Closes the Kuzu connection.
        /// </summary>
        public async Task CloseAsync()
        {
            try
            {
                if (_conn != null)
                {
                    await Task.Run(() => kuzu_connection_destroy(_conn));
                    _conn = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while closing the Kuzu connection: {e.Message}");
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
                    kuzu_value relatedNode = new kuzu_value();
                    kuzu_value relationship = new kuzu_value();

                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 0, originalNode));
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 1, relatedNode));
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 2, relationship));

                    kuzu_logical_type originalNodeDataType = new kuzu_logical_type();
                    kuzu_logical_type relatedNodeDataType = new kuzu_logical_type();
                    kuzu_logical_type relationshipDataType = new kuzu_logical_type();

                    await Task.Run(() => kuzu_value_get_data_type(originalNode, originalNodeDataType));
                    await Task.Run(() => kuzu_value_get_data_type(relatedNode, relatedNodeDataType));
                    await Task.Run(() => kuzu_value_get_data_type(relationship, relationshipDataType));

                    if (!(kuzu_data_type_get_id(originalNodeDataType) == kuzu_data_type_id.KUZU_STRING && kuzu_data_type_get_id(relatedNodeDataType) == kuzu_data_type_id.KUZU_STRING && kuzu_data_type_get_id(relationshipDataType) == kuzu_data_type_id.KUZU_STRING))
                    {
                        string originalNodeColumnName = string.Empty;
                        string relatedNodeColumnName = string.Empty;
                        string relationshipColumnName = string.Empty;
                        await Task.Run(() => kuzu_query_result_get_column_name(result, 0, out originalNodeColumnName));
                        await Task.Run(() => kuzu_query_result_get_column_name(result, 1, out relatedNodeColumnName));
                        await Task.Run(() => kuzu_query_result_get_column_name(result, 2, out relationshipColumnName));

                        sb.AppendLine($"{originalNodeColumnName}: {await GetValueStringAsync(originalNode, originalNodeDataType)}");

                        sb.AppendLine($"{relationshipColumnName}: {await GetValueStringAsync(relationship, relationshipDataType)}");

                        sb.AppendLine($"{relatedNodeColumnName}: {await GetValueStringAsync(relatedNode, relatedNodeDataType)}");

                        for (ulong i = 2; i < numColumns; i++)
                        {
                            kuzu_value columnValue = new kuzu_value();
                            await Task.Run(() => kuzu_flat_tuple_get_value(tuple, i, columnValue));
                            string columnName = string.Empty;
                            await Task.Run(() => kuzu_query_result_get_column_name(result, i, out columnName));
                            kuzu_logical_type columnDataType = new kuzu_logical_type();
                            await Task.Run(() => kuzu_value_get_data_type(columnValue, columnDataType));
                            if (!(columnName == "Band_related_node"))
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
                        if (!(columnName == "Band_related_node"))
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
            if (await Task.Run(() => kuzu_value_is_null(value)))
            {
                return "NULL";
            }

            var dataTypeId = await Task.Run(() => kuzu_data_type_get_id(dataType));

            switch (dataTypeId)
            {
                case kuzu_data_type_id.KUZU_STRING:
                    string strValue = await Task<string>.Factory.StartNew(() =>
                    {
                        kuzu_value_get_string(value, out string result);
                        return result;
                    });
                    return strValue;

                case kuzu_data_type_id.KUZU_INT64:
                    long intValue = await Task<long>.Factory.StartNew(() =>
                    {
                        kuzu_value_get_int64(value, out long result);
                        return result;
                    });
                    return intValue.ToString();

                case kuzu_data_type_id.KUZU_INT32:
                    int intValue32 = await Task<int>.Factory.StartNew(() =>
                    {
                        kuzu_value_get_int32(value, out int result);
                        return result;
                    });
                    return intValue32.ToString();

                case kuzu_data_type_id.KUZU_FLOAT:
                    float floatValue = await Task<float>.Factory.StartNew(() =>
                    {
                        kuzu_value_get_float(value, out float result);
                        return result;
                    });
                    return floatValue.ToString();

                case kuzu_data_type_id.KUZU_DOUBLE:
                    double doubleValue = await Task<double>.Factory.StartNew(() =>
                    {
                        kuzu_value_get_double(value, out double result);
                        return result;
                    });
                    return doubleValue.ToString();

                case kuzu_data_type_id.KUZU_BOOL:
                    bool boolValue = await Task<bool>.Factory.StartNew(() =>
                    {
                        kuzu_value_get_bool(value, out bool result);
                        return result;
                    });
                    return boolValue.ToString();

                case kuzu_data_type_id.KUZU_NODE:
                    return await Task.Run(() =>
                    {
                        StringBuilder sb = new StringBuilder();

                        kuzu_internal_id_t internalNodeId = new kuzu_internal_id_t();

                        kuzu_value idValue = new kuzu_value();
                        kuzu_node_val_get_id_val(value, idValue);
                        kuzu_value_get_internal_id(idValue, internalNodeId);

                        sb.AppendLine($"  _ID: {internalNodeId.ToString()}");

                        kuzu_value labelValue = new kuzu_value();
                        kuzu_node_val_get_label_val(value, labelValue);
                        kuzu_value_get_string(labelValue, out string label);

                        kuzu_node_val_get_property_size(value, out ulong propertyCount);

                        sb.AppendLine($"  _LABEL: {label},");

                        for (ulong i = 0; i < propertyCount; i++)
                        {
                            kuzu_value propertyValue = new kuzu_value();
                            kuzu_node_val_get_property_value_at(value, i, propertyValue);

                            kuzu_node_val_get_property_name_at(value, i, out string propertyName);

                            kuzu_logical_type propertyDataType = new kuzu_logical_type();
                            kuzu_value_get_data_type(propertyValue, propertyDataType);
                            var dataTypeId = kuzu_data_type_get_id(propertyDataType);

                            switch (dataTypeId)
                            {
                                case kuzu_data_type_id.KUZU_STRING:
                                    kuzu_value_get_string(propertyValue, out string propertyStringValue);
                                    sb.AppendLine($"  {propertyName}: \"{propertyStringValue}\",");
                                    break;

                                case kuzu_data_type_id.KUZU_BOOL:
                                    kuzu_value_get_bool(propertyValue, out bool boolValue);
                                    sb.AppendLine($"  {propertyName}: {boolValue},");
                                    break;

                                case kuzu_data_type_id.KUZU_INT8:
                                    kuzu_value_get_int8(propertyValue, out sbyte int8Value);
                                    sb.AppendLine($"  {propertyName}: {int8Value},");
                                    break;

                                case kuzu_data_type_id.KUZU_INT16:
                                    kuzu_value_get_int16(propertyValue, out short int16Value);
                                    sb.AppendLine($"  {propertyName}: {int16Value},");
                                    break;

                                case kuzu_data_type_id.KUZU_INT32:
                                    kuzu_value_get_int32(propertyValue, out int int32Value);
                                    sb.AppendLine($"  {propertyName}: {int32Value},");
                                    break;

                                case kuzu_data_type_id.KUZU_INT64:
                                    kuzu_value_get_int64(propertyValue, out long int64Value);
                                    sb.AppendLine($"  {propertyName}: {int64Value},");
                                    break;

                                case kuzu_data_type_id.KUZU_UINT8:
                                    kuzu_value_get_uint8(propertyValue, out byte uint8Value);
                                    sb.AppendLine($"  {propertyName}: {uint8Value},");
                                    break;

                                case kuzu_data_type_id.KUZU_UINT16:
                                    kuzu_value_get_uint16(propertyValue, out ushort uint16Value);
                                    sb.AppendLine($"  {propertyName}: {uint16Value},");
                                    break;

                                case kuzu_data_type_id.KUZU_UINT32:
                                    kuzu_value_get_uint32(propertyValue, out uint uint32Value);
                                    sb.AppendLine($"  {propertyName}: {uint32Value},");
                                    break;

                                case kuzu_data_type_id.KUZU_UINT64:
                                    kuzu_value_get_uint64(propertyValue, out ulong uint64Value);
                                    sb.AppendLine($"  {propertyName}: {uint64Value},");
                                    break;

                                case kuzu_data_type_id.KUZU_INT128:
                                    kuzu_int128_t int128Value = new kuzu_int128_t();
                                    kuzu_value_get_int128(propertyValue, int128Value);
                                    sb.AppendLine($"  {propertyName}: {int128Value}");
                                    break;

                                case kuzu_data_type_id.KUZU_FLOAT:
                                    kuzu_value_get_float(propertyValue, out float floatValue);
                                    sb.AppendLine($"  {propertyName}: {floatValue},");
                                    break;

                                case kuzu_data_type_id.KUZU_DOUBLE:
                                    kuzu_value_get_double(propertyValue, out double doubleValue);
                                    sb.AppendLine($"  {propertyName}: {doubleValue},");
                                    break;

                                case kuzu_data_type_id.KUZU_INTERNAL_ID:
                                    kuzu_internal_id_t internalId = new kuzu_internal_id_t();
                                    kuzu_value_get_internal_id(propertyValue, internalId);
                                    sb.AppendLine($"  {propertyName}: {internalId}");
                                    break;

                                case kuzu_data_type_id.KUZU_DATE:
                                    kuzu_date_t dateValue = new kuzu_date_t();
                                    kuzu_value_get_date(propertyValue, dateValue);
                                    sb.AppendLine($"  {propertyName}: {dateValue}");
                                    break;

                                case kuzu_data_type_id.KUZU_TIMESTAMP:
                                    kuzu_timestamp_t timestampValue = new kuzu_timestamp_t();
                                    kuzu_value_get_timestamp(propertyValue, timestampValue);
                                    sb.AppendLine($"  {propertyName}: {timestampValue}");
                                    break;

                                case kuzu_data_type_id.KUZU_BLOB:
                                    kuzu_value_get_blob(propertyValue, out byte[] blobValue);
                                    sb.AppendLine($"  {propertyName}: <blob size: {blobValue.Length}>");
                                    break;

                                case kuzu_data_type_id.KUZU_UUID:
                                    kuzu_value_get_uuid(propertyValue, out string uuidValue);
                                    sb.AppendLine($"  {propertyName}: {uuidValue}");
                                    break;
                                default:
                                    sb.AppendLine($"  {propertyName}: <unsupported type>,");
                                    break;
                            }
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
                    StringBuilder sbRecursive = new StringBuilder();

                    kuzu_value nodeList = new kuzu_value();
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

                    for (ulong j = 1; j < relationCount; j++)
                    {
                        kuzu_value currentRelation = new kuzu_value();
                        kuzu_value_get_list_element(relationshipList, j, currentRelation);

                        kuzu_logical_type relationDataType = new kuzu_logical_type();
                        await Task.Run(() => kuzu_value_get_data_type(currentRelation, relationDataType));

                        sbRecursive.AppendLine($"Relationship: {await GetValueStringAsync(currentRelation, relationDataType)}");
                    }

                    return sbRecursive.ToString();

                default:
                    return "Unsupported data type";
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
