using System;
using static kuzunet;
using System.Text;
using System.Data;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.graph
{
    public class KuzuDatabaseManager : IKuzuDatabaseManager
    {
        private readonly string _kuzuDbPath = "../deeplynx.graph/kuzu_db";
        private kuzu_database _db;
        private kuzu_connection? _conn;


        /// <summary>
        /// Initializes a new instance of the KuzuDataBusiness class.
        /// </summary>
        /// <param name="kuzuDbPath">Path to the Kuzu database.</param>
        public KuzuDatabaseManager()
        {
            _db = new kuzu_database();
            _conn = new kuzu_connection();
        }


        /// <summary>
        /// Connects to the Kuzu database.
        /// </summary>
        public async Task ConnectAsync()
        {
            try
            {
                kuzu_system_config config = kuzu_default_system_config();
                var state = await Task.Run(() => kuzu_database_init(_kuzuDbPath, config, _db));

                if (state == kuzu_state.KuzuError)
                {
                    Console.WriteLine("Could not create DB");
                    return;
                }

                state = await Task.Run(() => kuzu_connection_init(_db, _conn));
                if (state == kuzu_state.KuzuError)
                {
                    Console.WriteLine("Could not connect to DB");
                    return;
                }

                Console.WriteLine("Connected to Kuzu database.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while connecting to the database: {e.Message}");
            }
        }



        /// <summary>
        /// Main export function that checks records and edges, assigns defaults, copies data, and indicates successful export.
        /// </summary>
        public async Task<bool> ExportDataAsync(string pgParams, int? project_id)
        {
            if (_conn == null)
            {
                Console.WriteLine("Not connected to Kuzu. Please call the Connect method first.");
                return false;
            }

            bool hasError = false;

            try
            {
                await Task.Run(() => PerformNonQueryAsync("INSTALL postgres;"));
                await Task.Run(() => PerformNonQueryAsync("LOAD EXTENSION postgres;"));
                await Task.Run(() => PerformNonQueryAsync("INSTALL json;"));
                await Task.Run(() => PerformNonQueryAsync("LOAD EXTENSION json;"));

                string attachCommand = $"ATTACH '{pgParams}' AS test (dbtype postgres, skip_unsupported_table = TRUE, schema = 'deeplynx');";
                await Task.Run(() => PerformNonQueryAsync(attachCommand));

                await SetupKuzuTablesAsync();
                hasError = await LoadDataAsync(project_id);

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
        /// Sets up the necessary tables in the Kuzu database.
        /// </summary>
        private async Task SetupKuzuTablesAsync()
        {
            try
            {
                await PerformNonQueryAsync("DROP TABLE IF EXISTS PERFORMS;");
                await PerformNonQueryAsync("DROP TABLE IF EXISTS MEMBER_OF;");
                await PerformNonQueryAsync("DROP TABLE IF EXISTS CONTAINS;");
                await PerformNonQueryAsync("DROP TABLE IF EXISTS RELATES_TO");

                await PerformNonQueryAsync("DROP TABLE IF EXISTS Song;");
                await PerformNonQueryAsync("CREATE NODE TABLE Song (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");
                await PerformNonQueryAsync("DROP TABLE IF EXISTS Album;");
                await PerformNonQueryAsync("CREATE NODE TABLE Album (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");
                await PerformNonQueryAsync("DROP TABLE IF EXISTS Musician;");
                await PerformNonQueryAsync("CREATE NODE TABLE Musician (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");
                await PerformNonQueryAsync("DROP TABLE IF EXISTS Band;");
                await PerformNonQueryAsync("CREATE NODE TABLE Band (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");
                await PerformNonQueryAsync("DROP TABLE IF EXISTS Entity;");
                await PerformNonQueryAsync("CREATE NODE TABLE Entity (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, class_name STRING, project_id INT64, PRIMARY KEY (id));");

                await PerformNonQueryAsync("CREATE REL TABLE RELATES_TO (FROM Musician TO Song, FROM Band TO Song, relationship_name STRING);");
                await PerformNonQueryAsync("CREATE REL TABLE PERFORMS (FROM Musician TO Song, FROM Band TO Song, relationship_name STRING);");
                await PerformNonQueryAsync("CREATE REL TABLE MEMBER_OF (FROM Musician TO Band, relationship_name STRING);");
                await PerformNonQueryAsync("CREATE REL TABLE CONTAINS (FROM Album TO Song, relationship_name STRING);");

                Console.WriteLine("Tables setup successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while setting up Kuzu tables: {e.Message}");
            }
        }

        /// <summary>
        /// Loads both records and edges into the Kuzu database for a specified project.
        /// This method first attempts to load all relevant records and then loads the
        /// edges that relate the records together based on the provided project ID.
        /// </summary>
        /// <param name="project_id">The project ID to filter the records and edges being loaded.</param>
        /// <returns>Returns true if there was an error during loading, otherwise false.</returns>
        public async Task<bool> LoadDataAsync(int? project_id)
        {
            bool hasError = false;

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
        /// This method performs a series of COPY commands to transfer data
        /// related to various classes (e.g., Band, Musician, etc.) and assigns
        /// defaults where necessary.
        /// </summary>
        /// <param name="project_id">The project ID to filter the records being loaded.</param>
        /// <returns>Returns true if there was an error during loading, otherwise false.</returns>
        private async Task<bool> LoadRecordsAsync(int? project_id)
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
        /// This method performs a series of COPY commands to transfer edges
        /// related to relationships between records and ensures that they are
        /// correctly assigned based on the specified project ID.
        /// </summary>
        /// <param name="project_id">The project ID to filter the edges being loaded.</param>
        /// <returns>Returns true if there was an error during loading, otherwise false.</returns>
        private async Task<bool> LoadEdgesAsync(int? project_id)
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
        public async Task<string> ExecuteQueryAsync(KuzuDatabaseManagerQueryRequestDto request)
        {
            kuzu_query_result result = new kuzu_query_result();

            if (_conn == null)
            {
                Console.WriteLine("Not connected to Kuzu. Please call the Connect method first.");
                return "Error: Not connected to Kuzu.";
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

                return await FormatQueryResultAsync(result);
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

                kuzu_value value = new kuzu_value();
                await Task.Run(() => kuzu_flat_tuple_get_value(tuple, 0, value));

                kuzu_logical_type columnDataType = new kuzu_logical_type();
                await Task.Run(() => kuzu_value_get_data_type(value, columnDataType));
                var columnDataTypeId = await Task.Run(() => kuzu_data_type_get_id(columnDataType));

                if (columnDataTypeId == kuzu_data_type_id.KUZU_NODE)
                {
                    sb.AppendLine(await GetValueStringAsync(value, columnDataType));
                }
                else
                {
                    for (ulong i = 0; i < numColumns; i++)
                    {
                        kuzu_value columnValue = new kuzu_value();
                        await Task.Run(() => kuzu_flat_tuple_get_value(tuple, i, columnValue));
                        string columnName = string.Empty;
                        await Task.Run(() => kuzu_query_result_get_column_name(result, i, out columnName));
                        sb.AppendLine($"{columnName}: {await GetValueStringAsync(columnValue, columnDataType)}");
                    }
                    sb.AppendLine();
                }

                await Task.Run(() => kuzu_flat_tuple_destroy(tuple));
            }

            return sb.ToString();
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
                case kuzu_data_type_id.KUZU_NODE:
                    return await Task.Run(() =>
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("NODE:\n");

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

                        sb.Append($"{{_ID: {id}, _LABEL: {label}, id: {originalId}, properties: {properties}, \"data_source_id\": {dataSourceId}, \"original_id\": \"{originalId}\"}}");

                        return sb.ToString();
                    });

                case kuzu_data_type_id.KUZU_REL:
                    return await Task.Run(() =>
                    {
                        Console.WriteLine("REL:\n");


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

                        return $"({srcId})-{{_LABEL: {relLabel}, _ID: {relId}}}->({dstId})";
                    });

                default:
                    Console.WriteLine(dataTypeId);

                    return "Unsupported data type";
            }
        }
    }
}
