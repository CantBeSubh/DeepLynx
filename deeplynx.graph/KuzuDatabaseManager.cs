using System;
using static kuzunet;
using System.Text;
using System.Data;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.Extensions.Configuration;

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

                await SetupKuzuTablesAsync();

                bool projectIdExists = await CheckIfProjectIdExistsAsync(project_id);

                if (projectIdExists)
                {
                    Console.WriteLine($"Project ID {project_id} has already been processed. Skipping data load.");
                    return true;
                }

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
        /// Sets up the necessary tables in the Kuzu database.
        /// </summary>
        private async Task SetupKuzuTablesAsync()
        {
            try
            {
                await PerformNonQueryAsync("CREATE NODE TABLE IF NOT EXISTS ProcessedProjectIds (project_id INT64 PRIMARY KEY);");

                await PerformNonQueryAsync("CREATE NODE TABLE IF NOT EXISTS Song (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");

                await PerformNonQueryAsync("CREATE NODE TABLE IF NOT EXISTS Album (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");

                await PerformNonQueryAsync("CREATE NODE TABLE IF NOT EXISTS Musician (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");

                await PerformNonQueryAsync("CREATE NODE TABLE IF NOT EXISTS Band (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");

                await PerformNonQueryAsync("CREATE NODE TABLE IF NOT EXISTS Entity (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");

                await PerformNonQueryAsync("CREATE REL TABLE IF NOT EXISTS RELATES_TO (FROM Musician TO Song, FROM Band TO Song, relationship_name STRING);");

                await PerformNonQueryAsync("CREATE REL TABLE IF NOT EXISTS PERFORMS (FROM Musician TO Song, FROM Band TO Song, relationship_name STRING);");

                await PerformNonQueryAsync("CREATE REL TABLE IF NOT EXISTS MEMBER_OF (FROM Musician TO Band, relationship_name STRING);");

                await PerformNonQueryAsync("CREATE REL TABLE IF NOT EXISTS CONTAINS (FROM Album TO Song, relationship_name STRING);");

                Console.WriteLine("Tables setup successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while setting up Kuzu tables: {e.Message}");
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
                    a AS {request.TableName}_node,
                    r AS RECURSIVE_RELATIONSHIP,
                    b AS {request.TableName}_related_node";

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
        /// Loads both records and edges into the Kuzu database for a specified project.
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
        /// Loads records from the PostgreSQL database into the KuzuDB.
        /// </summary>
        /// <param name="project_id">The project ID to filter the records being loaded.</param>
        /// <returns>Returns true if there was an error during loading, otherwise false.</returns>
        private async Task<bool> LoadRecordsAsync(int project_id)
        {
            bool hasError = false;

            try
            {
                await PerformNonQueryAsync($"COPY Band FROM (LOAD FROM historical_records WHERE class_name = 'band' AND project_id = {project_id} RETURN id, properties, data_source_id, original_id, name, class_name, project_id);");
                await PerformNonQueryAsync($"COPY Musician FROM (LOAD FROM historical_records WHERE class_name = 'musician' AND project_id = {project_id} RETURN id, properties, data_source_id, original_id, name, class_name, project_id);");
                await PerformNonQueryAsync($"COPY Song FROM (LOAD FROM historical_records WHERE class_name = 'song' AND project_id = {project_id} RETURN id, properties, data_source_id, original_id, name, class_name, project_id);");
                await PerformNonQueryAsync($"COPY Album FROM (LOAD FROM historical_records WHERE class_name = 'album' AND project_id = {project_id} RETURN id, properties, data_source_id, original_id, name, class_name, project_id);");
                await PerformNonQueryAsync($"COPY Entity FROM (LOAD FROM historical_records WHERE class_name IS NULL AND project_id = {project_id} RETURN id, properties, data_source_id, original_id, name, class_name, project_id);");

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
        /// Loads edges from the edges_c view into the KuzuDB.
        /// </summary>
        /// <param name="project_id">The project ID to filter the edges being loaded.</param>
        /// <returns>Returns true if there was an error during loading, otherwise false.</returns>
        private async Task<bool> LoadEdgesAsync(int project_id)
        {
            bool hasError = false;

            try
            {
                await PerformNonQueryAsync($"COPY RELATES_TO FROM (LOAD FROM edges_c WHERE relationship_name IS NULL AND project_id = {project_id} AND orig_class = 'musician' RETURN origin_id AS FROM, destination_id AS TO, relationship_name) (from='Musician', to='Song');");
                await PerformNonQueryAsync($"COPY RELATES_TO FROM (LOAD FROM edges_c WHERE relationship_name IS NULL AND project_id = {project_id} AND orig_class = 'band' RETURN origin_id AS FROM, destination_id AS TO, relationship_name) (from='Band', to='Song');");
                await PerformNonQueryAsync($"COPY PERFORMS FROM (LOAD FROM edges_c WHERE relationship_name = 'performs' AND orig_class = 'musician' AND project_id = {project_id} RETURN origin_id AS FROM, destination_id AS TO, 'performs' AS relationship_name) (from='Musician', to='Song');");
                await PerformNonQueryAsync($"COPY PERFORMS FROM (LOAD FROM edges_c WHERE relationship_name = 'performs' AND orig_class = 'band' AND project_id = {project_id} RETURN origin_id AS FROM, destination_id AS TO, 'performs' AS relationship_name) (from='Band', to='Song');");
                await PerformNonQueryAsync($"COPY MEMBER_OF FROM (LOAD FROM edges_c WHERE relationship_name = 'member_of' AND project_id = {project_id} RETURN origin_id, destination_id, relationship_name);");
                await PerformNonQueryAsync($"COPY CONTAINS FROM (LOAD FROM edges_c WHERE relationship_name = 'contains' AND project_id = {project_id} RETURN origin_id, destination_id, relationship_name);");

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

                ulong numTuples = kuzu_query_result_get_num_tuples(result);
                Console.WriteLine($"Number of tuples returned: {numTuples}");

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
                    kuzu_value bandNode = new kuzu_value();
                    kuzu_value relatedNode = new kuzu_value();
                    kuzu_value relationship = new kuzu_value();

                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 0, bandNode));
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 1, relatedNode));
                    await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 2, relationship));

                    kuzu_logical_type bandDataType = new kuzu_logical_type();
                    kuzu_logical_type relatedDataType = new kuzu_logical_type();
                    kuzu_logical_type relationshipDataType = new kuzu_logical_type();

                    await Task.Run(() => kuzu_value_get_data_type(bandNode, bandDataType));
                    await Task.Run(() => kuzu_value_get_data_type(relatedNode, relatedDataType));
                    await Task.Run(() => kuzu_value_get_data_type(relationship, relationshipDataType));

                    if (!(kuzu_data_type_get_id(bandDataType) == kuzu_data_type_id.KUZU_STRING && kuzu_data_type_get_id(relatedDataType) == kuzu_data_type_id.KUZU_STRING && kuzu_data_type_get_id(relationshipDataType) == kuzu_data_type_id.KUZU_STRING))
                    {
                        string bandColumnName = string.Empty;
                        string relatedColumnName = string.Empty;
                        string relationshipColumnName = string.Empty;
                        await Task.Run(() => kuzu_query_result_get_column_name(result, 0, out bandColumnName));
                        await Task.Run(() => kuzu_query_result_get_column_name(result, 1, out relatedColumnName));
                        await Task.Run(() => kuzu_query_result_get_column_name(result, 2, out relationshipColumnName));

                        sb.AppendLine($"{bandColumnName}: {await GetValueStringAsync(bandNode, bandDataType)}");

                        sb.AppendLine($"{relationshipColumnName}: {await GetValueStringAsync(relationship, relationshipDataType)}");

                        sb.AppendLine($"{relatedColumnName}: {await GetValueStringAsync(relatedNode, relatedDataType)}");

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

                        kuzu_value idValue = new kuzu_value();
                        kuzu_node_val_get_id_val(value, idValue);
                        kuzu_value_get_int64(idValue, out long id);

                        kuzu_value labelValue = new kuzu_value();
                        kuzu_node_val_get_label_val(value, labelValue);
                        kuzu_value_get_string(labelValue, out string label);

                        kuzu_value nameValue = new kuzu_value();
                        kuzu_node_val_get_property_value_at(value, 4, nameValue);
                        kuzu_value_get_string(nameValue, out string name);

                        kuzu_value yearValue = new kuzu_value();
                        kuzu_node_val_get_property_value_at(value, 0, yearValue);
                        kuzu_value_get_int64(yearValue, out long year);

                        kuzu_value propertiesValue = new kuzu_value();
                        kuzu_node_val_get_property_value_at(value, 1, propertiesValue);
                        kuzu_value_get_string(propertiesValue, out string properties);

                        kuzu_value dataSourceIdValue = new kuzu_value();
                        kuzu_node_val_get_property_value_at(value, 2, dataSourceIdValue);
                        kuzu_value_get_int64(dataSourceIdValue, out long dataSourceId);

                        kuzu_value originalIdValue = new kuzu_value();
                        kuzu_node_val_get_property_value_at(value, 3, originalIdValue);
                        kuzu_value_get_string(originalIdValue, out string originalId);

                        sb.AppendLine("{");
                        sb.AppendLine($"  _ID: {id},");
                        sb.AppendLine($"  _LABEL: {label},");
                        sb.AppendLine($"  id: {originalId},");
                        sb.AppendLine($"  properties: {properties},");
                        sb.AppendLine($"  data_source_id: {dataSourceId},");
                        sb.AppendLine($"  original_id: \"{originalId}\"");
                        sb.AppendLine("}");

                        return sb.ToString();
                    });

                case kuzu_data_type_id.KUZU_REL:
                    return await Task.Run(() =>
                    {
                        kuzu_value srcIdValue = new kuzu_value();
                        kuzu_rel_val_get_src_id_val(value, srcIdValue);
                        kuzu_value_get_int64(srcIdValue, out long srcId);

                        kuzu_value dstIdValue = new kuzu_value();
                        kuzu_rel_val_get_dst_id_val(value, dstIdValue);
                        kuzu_value_get_int64(dstIdValue, out long dstId);

                        kuzu_value labelRelValue = new kuzu_value();
                        kuzu_rel_val_get_label_val(value, labelRelValue);
                        kuzu_value_get_string(labelRelValue, out string relLabel);

                        kuzu_value relIdValue = new kuzu_value();
                        kuzu_rel_val_get_property_value_at(value, 0, relIdValue);
                        kuzu_value_get_int64(relIdValue, out long relId);

                        return $"{{ _LABEL: {relLabel}}}";
                    });

                case kuzu_data_type_id.KUZU_RECURSIVE_REL:
                    StringBuilder sbRecursive = new StringBuilder();
                    sbRecursive.AppendLine("\n");

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

                        sbRecursive.AppendLine($"Node {i}: {await GetValueStringAsync(currentNode, nodeDataType)}");
                    }

                    ulong relationCount;
                    kuzu_value_get_list_size(relationshipList, out relationCount);

                    for (ulong j = 1; j < relationCount; j++)
                    {
                        kuzu_value currentRelation = new kuzu_value();
                        kuzu_value_get_list_element(relationshipList, j, currentRelation);

                        kuzu_logical_type relationDataType = new kuzu_logical_type();
                        await Task.Run(() => kuzu_value_get_data_type(currentRelation, relationDataType));

                        sbRecursive.AppendLine($"Relationship {j}: {await GetValueStringAsync(currentRelation, relationDataType)}");
                    }

                    return sbRecursive.ToString();

                default:
                    return "Unsupported data type";
            }
        }
    }
}
