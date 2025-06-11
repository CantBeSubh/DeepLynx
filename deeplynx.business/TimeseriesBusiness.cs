using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using DuckDB.NET.Data;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class TimeseriesBusiness : ITimeseriesBusiness
{
    private readonly DeeplynxContext _context;

    public TimeseriesBusiness(DeeplynxContext context)
    {
    }

    // todo: get methods implemented here

    public DuckDBConnection GetDuckDBConnection()
    {
        return new DuckDBConnection("Data Source=file.db");
    }

    public List<List<dynamic>> OrganizeQueryData(DuckDBDataReader reader)
    {
        List<dynamic> columns = [];
        for (var index = 0; index < reader.FieldCount; index++)
        {
            string column = reader.GetName(index);
            columns.Add(column);
        }

        List<List<dynamic>> tableData = [columns];

        while (reader.Read())
        {
            for (int ordinal = 0; ordinal < reader.FieldCount; ordinal += columns.Count)
            {
                List<dynamic> data = [];
                for (int i = 0; i < columns.Count; i++)
                {
                    int newOrdinal = ordinal + i;

                    if (reader.IsDBNull(ordinal))
                    {
                        Console.WriteLine("NULL");
                        data.Add(null);
                    }
                    else
                    {
                        data.Add(reader.GetValue(newOrdinal));
                    }
                }
                tableData.Add(data);
            }
        }

        return tableData;
    }

    public async Task<List<List<dynamic>>> QueryTimeseries(string query)
    {
        using DuckDBConnection duckDBConnection = GetDuckDBConnection();
        await duckDBConnection.OpenAsync();
        using DuckDBCommand command = duckDBConnection.CreateCommand();

        command.CommandText = query;
        using DuckDBDataReader reader = command.ExecuteReader();

        return OrganizeQueryData(reader);
    }

    public async Task<List<List<dynamic>>> GetAllTableRecords(TimeseriesDataDto timeSeriesDataDTO)
    {
        using DuckDBConnection duckDBConnection = GetDuckDBConnection();
        await duckDBConnection.OpenAsync();
        using DuckDBCommand command = duckDBConnection.CreateCommand();

        command.CommandText = "SELECT * FROM $table_name";
        command.Parameters.Add(new DuckDBParameter("table_name", timeSeriesDataDTO.FileName));
        using DuckDBDataReader reader = command.ExecuteReader();

        return OrganizeQueryData(reader);
    }

    public async Task ProcessTimeSeriesDataAsync(TimeseriesDataDto timeSeriesDataDTO)
    {
        using DuckDBConnection duckDBConnection = GetDuckDBConnection();
        await duckDBConnection.OpenAsync();
        //Transaction? duckDBConnection.BeginTransaction();
        
        using DuckDBCommand command = duckDBConnection.CreateCommand();
        int executeNonQuery;

        //TODO What to do if table already exists
        command.CommandText = "CREATE TABLE $file_name AS SELECT * from '$file_name';"; //https://duckdb.org/docs/stable/data/overview.html
        command.Parameters.Add(new DuckDBParameter("$file_name", timeSeriesDataDTO.FilePath));
        executeNonQuery = command.ExecuteNonQuery();
    }
}
