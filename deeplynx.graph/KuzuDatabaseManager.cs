using System;
using static kuzunet;

namespace deeplynx.graph
{
    public class KuzuDatabaseManager
    {
        private readonly string _kuzuDbPath;
        private kuzu_database _db;
        private kuzu_connection? _conn;


        /// <summary>
        /// Initializes a new instance of the KuzuDataBusiness class.
        /// </summary>
        /// <param name="kuzuDbPath">Path to the Kuzu database.</param>
        public KuzuDatabaseManager(string kuzuDbPath)
        {
            _kuzuDbPath = kuzuDbPath;
            _db = new kuzu_database();
            _conn = new kuzu_connection();
        }


        /// <summary>
        /// Connects to the Kuzu database.
        /// </summary>
        public void Connect()
        {
            try
            {
                kuzu_system_config config = kuzu_default_system_config();
                var state = kuzu_database_init(_kuzuDbPath, config, _db);

                if (state == kuzu_state.KuzuError)
                {
                    Console.WriteLine("Could not create DB");
                    return;
                }

                state = kuzu_connection_init(_db, _conn);
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
        /// Sets up the Kuzu database with the attached PostgreSQL database.
        /// </summary>
        /// <param name="pgParams">The PostgreSQL connection string.</param>
        public void SetupKuzuWithPostgres(string pgParams)
        {
            using kuzu_query_result result = new();

            if (_conn == null)
            {
                Console.WriteLine("Not connected to Kuzu. Please call the Connect method first.");
                return;
            }

            try
            {
                PerformNonQuery("INSTALL postgres;");
                PerformNonQuery("LOAD EXTENSION postgres;");
                PerformNonQuery("INSTALL json;");
                PerformNonQuery("LOAD EXTENSION json;");

                string attachCommand = $"ATTACH '{pgParams}' AS test (dbtype postgres, skip_unsupported_table = TRUE, schema = 'deeplynx');";
                PerformNonQuery(attachCommand);

                SetupKuzuTables();
                LoadData();
                PerformNonQuery("DETACH test;");
                Console.WriteLine("Kuzu database setup with PostgreSQL completed successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred during the setup: {e.Message}");
            }
        }


        /// <summary>
        /// Sets up the necessary tables in the Kuzu database.
        /// </summary>
        private void SetupKuzuTables()
        {
            try
            {
                PerformNonQuery("DROP TABLE IF EXISTS Song;");
                PerformNonQuery("CREATE NODE TABLE Song (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, PRIMARY KEY (id));");
                PerformNonQuery("DROP TABLE IF EXISTS Album;");
                PerformNonQuery("CREATE NODE TABLE Album (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, PRIMARY KEY (id));");
                PerformNonQuery("DROP TABLE IF EXISTS Musician;");
                PerformNonQuery("CREATE NODE TABLE Musician (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, PRIMARY KEY (id));");
                PerformNonQuery("DROP TABLE IF EXISTS Band;");
                PerformNonQuery("CREATE NODE TABLE Band (id INT64, properties STRING, data_source_id INT64, original_id STRING, name STRING, PRIMARY KEY (id));");

                PerformNonQuery("CREATE REL TABLE PERFORMS (FROM Musician TO Song, FROM Band TO Song);");
                PerformNonQuery("CREATE REL TABLE MEMBER_OF (FROM Musician TO Band);");
                PerformNonQuery("CREATE REL TABLE CONTAINS (FROM Album TO Song);");

                Console.WriteLine("Tables setup successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while setting up Kuzu tables: {e.Message}");
            }
        }


        /// <summary>
        /// Loads data from PostgreSQL into KuzuDB.
        /// </summary>
        public void LoadData()
        {
            try
            {
                PerformNonQuery("COPY Band FROM (LOAD FROM records WHERE class_name = 'band' RETURN id, properties, data_source_id, original_id, name);");
                PerformNonQuery("COPY Musician FROM (LOAD FROM records WHERE class_name = 'musician' RETURN id, properties, data_source_id, original_id, name);");
                PerformNonQuery("COPY Song FROM (LOAD FROM records WHERE class_name = 'song' RETURN id, properties, data_source_id, original_id, name);");
                PerformNonQuery("COPY Album FROM (LOAD FROM records WHERE class_name = 'album' RETURN id, properties, data_source_id, original_id, name);");

                PerformNonQuery("COPY PERFORMS FROM (LOAD FROM edges_c WHERE relationship_name = 'performs' AND orig_class = 'musician' RETURN origin_id, destination_id) (from='Musician', to='Song');");
                PerformNonQuery("COPY PERFORMS FROM (LOAD FROM edges_c WHERE relationship_name = 'performs' AND orig_class = 'band' RETURN origin_id, destination_id) (from='Band', to='Song');");
                PerformNonQuery("COPY MEMBER_OF FROM (LOAD FROM edges_c WHERE relationship_name = 'member_of' RETURN origin_id, destination_id);");
                PerformNonQuery("COPY CONTAINS FROM (LOAD FROM edges_c WHERE relationship_name = 'contains' RETURN origin_id, destination_id);");

                Console.WriteLine("Data loaded successfully into KuzuDB.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while loading data: {e.Message}");
            }
        }


        /// <summary>
        /// Closes the Kuzu connection.
        /// </summary>
        public void Close()
        {
            try
            {
                if (_conn != null)
                {
                    kuzu_connection_destroy(_conn);
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
        public string ExecuteQuery(string query)
        {

            kuzu_query_result result = new kuzu_query_result();

            if (_conn == null)
            {
                Console.WriteLine("Not connected to Kuzu. Please call the Connect method first.");
                return "Error: Not connected to Kuzu.";
            }

            try
            {
                var state = kuzu_connection_query(_conn, query, result);

                if (state == kuzu_state.KuzuError)
                {
                    string errorMessage = kuzu_query_result_get_error_message(result);
                    Console.WriteLine($"Error executing query: {query}");
                    Console.WriteLine($"Error Message: {errorMessage}");
                    return $"Error: {errorMessage}";
                }

                return FormatQueryResult(result);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while executing the query: {e.Message}");
                return $"Error: {e.Message}";
            }
        }


        /// <summary>
        /// Executes a non-query command against the Kuzu database.
        /// </summary>
        /// <param name="query">The Cypher query string to be executed.</param>
        private void PerformNonQuery(string query)
        {
            try
            {
                using kuzu_query_result result = new();
                var state = kuzu_connection_query(_conn, query, result);
                if (state == kuzu_state.KuzuError)
                {
                    Console.WriteLine("Could not perform: " + query);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while performing the query: {e.Message}");
            }
        }

        /// <summary>
        /// Formats the Kuzu query result into a readable string.
        /// </summary>
        /// <param name="result">The result object containing the query results.</param>
        /// <returns>A string representation of the formatted query results.</returns>
        public string FormatQueryResult(kuzu_query_result result)
        {
            ulong numColumns = kuzu_query_result_get_num_columns(result);

            string[] columnNames = new string[numColumns];
            for (ulong i = 0; i < numColumns; i++)
            {
                kuzu_query_result_get_column_name(result, i, out columnNames[i]);
            }

            StringWriter sw = new StringWriter();

            while (kuzu_query_result_has_next(result))
            {
                kuzu_flat_tuple tuple = new kuzu_flat_tuple();
                kuzu_query_result_get_next(result, tuple);

                for (ulong i = 0; i < numColumns; i++)
                {
                    kuzu_logical_type columnDataType = new kuzu_logical_type();
                    kuzu_query_result_get_column_data_type(result, i, columnDataType);

                    kuzu_value value = new kuzu_value();
                    kuzu_flat_tuple_get_value(tuple, i, value);

                    sw.WriteLine($"{columnNames[i]}: {GetValueString(value, columnDataType)}");
                    kuzu_value_destroy(value);
                }
                sw.WriteLine();
                kuzu_flat_tuple_destroy(tuple);
            }

            return sw.ToString();
        }

        /// <summary>
        /// Converts a Kuzu value into its string representation based on its logical type.
        /// </summary>
        /// <param name="value">The Kuzu value to be converted.</param>
        /// <param name="dataType">The logical type of the Kuzu value.</param>
        /// <returns>A string representation of the value. If the value is null, returns "NULL".</returns>
        private static string GetValueString(kuzu_value value, kuzu_logical_type dataType)
        {
            if (kuzu_value_is_null(value))
            {
                return "NULL";
            }

            var dataTypeId = kuzu_data_type_get_id(dataType);

            switch (dataTypeId)
            {
                case kuzu_data_type_id.KUZU_STRING:
                    kuzu_value_get_string(value, out string strValue);
                    return strValue;
                case kuzu_data_type_id.KUZU_INT64:
                    kuzu_value_get_int64(value, out long intValue);
                    return intValue.ToString();
                case kuzu_data_type_id.KUZU_INT32:
                    kuzu_value_get_int32(value, out int intValue32);
                    return intValue32.ToString();
                case kuzu_data_type_id.KUZU_FLOAT:
                    kuzu_value_get_float(value, out float floatValue);
                    return floatValue.ToString();
                case kuzu_data_type_id.KUZU_DOUBLE:
                    kuzu_value_get_double(value, out double doubleValue);
                    return doubleValue.ToString();
                default:
                    return "Unsupported data type";
            }
        }
    }
}
