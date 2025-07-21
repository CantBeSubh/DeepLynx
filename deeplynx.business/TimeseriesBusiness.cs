using System.Data;
using System.Text;
using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using DuckDB.NET.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace deeplynx.business;

public class TimeseriesBusiness(DeeplynxContext context, IRecordBusiness recordBusiness, IClassBusiness classBusiness, [FromServices] IServiceScopeFactory serviceScopeFactory) : ITimeseriesBusiness
{
    private readonly DeeplynxContext _context = context;
    private readonly IRecordBusiness _recordBusiness = recordBusiness;
    private readonly IClassBusiness _classBusiness = classBusiness;
    private IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    private const string UploadFolderPath = "uploads";
    private const string QueryFolderPath = "reports";

    private static class Status
    {
        public static string Failed { get; } = "failed";
        public static string Completed { get; } = "completed";
        public static string InProgress { get; } = "in progress";
    }

    /// <summary>
    /// Uploads a time series file and kicks off the processing for DuckDB
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The Data Source ID</param>
    /// <param name="file">This is the entire file attached as form data in the request</param>
    /// <returns>An object of the uploaded file information</returns>
    /// <exception cref="ArgumentException">If the file is null or has no data</exception>
    /// <exception cref="InvalidOperationException">If the server cannot create the directory</exception>
    public async Task<RecordResponseDto> UploadFile(long projectId, long dataSourceId, IFormFile file)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required and cannot be empty or whitespace.");
        }

        var uploadId = Guid.NewGuid().ToString();
        string tableName = uploadId + "_" + file.FileName;
        var filePath = Path.Combine(UploadFolderPath, projectId.ToString(), dataSourceId.ToString(), uploadId + "_" + file.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("error creating upload path"));
        var uri = "duckdb://" + tableName;

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        await CreateTimeseriesTable(tableName, filePath);

        var recordClass = await _classBusiness.GetClassInfo(projectId, "Timeseries");
        var columns = await GetColumnsFromDb(tableName);

        var recordRequest = new RecordRequestDto
        {
            Properties = new JsonObject
            {
                ["columns"] = columns,
                ["timeUploaded"] = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ["fileType"] = Path.GetExtension(file.FileName).TrimStart('.').ToLower()
            },
            Name = tableName,
            OriginalId = uploadId,
            Uri = uri,
            ClassId = recordClass.Id,
            ClassName = recordClass.Name,
        };

        return await _recordBusiness.CreateRecord(projectId, dataSourceId, recordRequest);
    }

    /// <summary>
    /// Sets up the directory for file chunks to be uploaded
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The Data Source ID</param>
    /// <returns>The upload ID (guid format) for file chunks to go to the right directory</returns>
    public string StartUpload(long projectId, long dataSourceId)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        var uploadId = Guid.NewGuid().ToString();
        var folderPath = Path.Combine(UploadFolderPath, projectId.ToString(), dataSourceId.ToString(), uploadId);
        Directory.CreateDirectory(folderPath);

        return uploadId;
    }

    /// <summary>
    /// Uploads a partial file to the specified directory
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The Data Source ID</param>
    /// <param name="chunk">Raw binary data, max of 30MB by default</param>
    /// <param name="uploadId">the upload guid from StartUpload</param>
    /// <param name="chunkNumber">the index for tracking the order to merge chunks together</param>
    /// <returns>A string to denote the status</returns>
    /// <exception cref="ArgumentException">If the chunk is null or has no data</exception>
    public async Task<string> UploadChunk(long projectId, long dataSourceId, IFormFile chunk,
        string uploadId, int chunkNumber)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        if (chunk == null || chunk.Length == 0)
        {
            throw new ArgumentException("No chunk uploaded.");
        }

        var tempFilePath = Path.Combine(UploadFolderPath, projectId.ToString(), dataSourceId.ToString(), uploadId, $"{chunkNumber}.part");
        await using var stream = new FileStream(tempFilePath, FileMode.Create);
        await chunk.CopyToAsync(stream);

        return "success";
    }

    /// <summary>
    /// Merges the file chunks and creates the finalized uploaded file and kicks off the processing for DuckDB
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The Data Source ID</param>
    /// <param name="request">The request, which contains the UploadID and FileName</param>
    /// <returns>An object of the uploaded file information</returns>
    public async Task<RecordResponseDto> CompleteUpload(long projectId, long dataSourceId,
        TimeseriesUploadCompleteRequestDto request)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        var folderPath = Path.Combine(UploadFolderPath, projectId.ToString(), dataSourceId.ToString(), request.UploadId);
        var tableName = request.UploadId + "_" + request.FileName;
        var finalFilePath = Path.Combine(UploadFolderPath, projectId.ToString(), dataSourceId.ToString(),
            request.UploadId + "_" + request.FileName);
        var uri = "duckdb://" + tableName;

        await using (var finalFileStream = new FileStream(finalFilePath, FileMode.Create))
        {
            for (var i = 0; i < request.TotalChunks; i++)
            {
                var partFilePath = Path.Combine(folderPath, $"{i}.part");
                await using (var partStream = new FileStream(partFilePath, FileMode.Open))
                {
                    await partStream.CopyToAsync(finalFileStream);
                }
                File.Delete(partFilePath); // Clean up the chunk file
            }
        }

        Directory.Delete(folderPath); // Clean up the upload folder

        // todo: kick off file processing here (See DL-97 Sub-Tasks)
        // start saving metadata to db
        // import into duckdb
        // describe table for metadata record properties
        // after processing, new filepath should be something like
        // "duckdb://path/to/uuid_filename"

        await CreateTimeseriesTable(tableName, finalFilePath);

        var recordClass = await _classBusiness.GetClassInfo(projectId, "Timeseries");
        var columns = await GetColumnsFromDb(tableName);

        var recordRequest = new RecordRequestDto
        {
            Properties = new JsonObject
            {
                ["columns"] = columns,
                ["timeUploaded"] = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ["fileType"] = Path.GetExtension(request.FileName).TrimStart('.').ToLower()
            },
            Name = tableName,
            OriginalId = request.UploadId,
            Uri = uri,
            ClassId = recordClass.Id,
            ClassName = recordClass.Name,
        };

        return await _recordBusiness.CreateRecord(projectId, dataSourceId, recordRequest);
    }

    /// <summary>
    /// This allows the user to query timeseries data in duckDb. Creates the command using an sql string
    /// The connection is read only so any write operations will be blocked.
    /// </summary>
    /// <param name="request"> The request which includes the query string</param>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The data source ID</param>
    /// <returns></returns>
    public async Task<RecordResponseDto> QueryTimeseries(TimeseriesQueryRequestDto request, long projectId, long dataSourceId)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        var resultTable = new DataTable();
        await using var duckDbConnection = GetReadOnlyDuckDbConnection();
        await duckDbConnection.OpenAsync();

        await using var command = duckDbConnection.CreateCommand();
        command.CommandText = request.Query;
        await using var reader = command.ExecuteReader();

        if (!reader.HasRows)
        {
            throw new NoResultsException("Empty query results, no report needed");
        }

        resultTable.Load(reader);
        var queryId = Guid.NewGuid().ToString();
        var fileName = queryId + "_record.csv";

        var reportClass = await _classBusiness.GetClassInfo(projectId, "Report");
        var recordRequest = new RecordRequestDto
        {
            Properties = new JsonObject
            {
                ["status"] = Status.InProgress,
                ["query"] = request.Query
            },
            Name = fileName,
            OriginalId = queryId,
            ClassId = reportClass.Id,
            ClassName = reportClass.Name
        };

        var recordResponse = await _recordBusiness.CreateRecord(projectId, dataSourceId, recordRequest);

        RunBackgroundJob(recordResponse, request, resultTable, projectId, dataSourceId, fileName);

        return recordResponse;
    }

    /// <summary>
    /// Runs a timeseries query to generate a csv from a DataTable
    /// </summary>
    /// <param name="recordResponse">The record response DTO</param>
    /// <param name="request">The timeseries query request DTO</param>
    /// <param name="resultTable">The table with time series records to be written</param>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The data source ID</param>
    /// <param name="fileName">The name of the file to be written</param>
    /// <exception cref="KeyNotFoundException">Thrown when the record cannot be found</exception>
    /// <exception cref="Exception">Thrown if the report cannot be written</exception>
    private void RunBackgroundJob(RecordResponseDto recordResponse, TimeseriesQueryRequestDto request, DataTable resultTable, long projectId, long dataSourceId, string fileName)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        // Runs in the background and lets the request finish
        // https://stackoverflow.com/questions/62222712/what-is-the-simplest-way-to-run-a-single-background-task-from-a-controller-in-n
        // todo: Write csv to object storage
        Task.Run(async () =>
        {
            // creates a background scope to create its own context so that the background task doesn't
            // have to rely on other contexts that may be destroyed or closed. 
            using var scope = _serviceScopeFactory.CreateScope();
            var backgroundContext = scope.ServiceProvider.GetRequiredService<DeeplynxContext>();

            try
            {
                DataTableToCsv(resultTable, projectId, dataSourceId, fileName);
                var properties = new JsonObject
                {
                    ["status"] = Status.Completed,
                    ["query"] = request.Query
                };
                var record = await backgroundContext.Records.FindAsync(recordResponse.Id);
                if (record == null || record.ProjectId != projectId || record.ArchivedAt != null)
                {
                    throw new KeyNotFoundException($"Record with id {recordResponse.Id} not found");
                }

                record.Properties = properties.ToString();
                record.Uri = "object://" + fileName;
                record.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                backgroundContext.Records.Update(record);
                await backgroundContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var properties = new JsonObject
                {
                    ["status"] = Status.Failed,
                    ["query"] = request.Query
                };
                var record = await backgroundContext.Records.FindAsync(recordResponse.Id);
                if (record == null || record.ProjectId != projectId || record.ArchivedAt != null)
                {
                    throw new KeyNotFoundException($"Record with id {recordResponse.Id} not found");
                }

                record.Properties = properties.ToString();

                backgroundContext.Records.Update(record);
                await backgroundContext.SaveChangesAsync();

                NLog.LogManager.GetCurrentClassLogger().Error(e);
                throw new Exception("Failed while writing report to csv and postgres");
            }
        });
    }

    //todo: Determine how to structure query result depending on how UI needs it. This function will be commented out for now

    /// <summary>
    /// Makes a JSON like response from the table. This will be replaced by a link to the csv later.
    /// </summary>
    /// <param name="dt"> data table with response data</param>
    /// <returns></returns>
    // private List<Dictionary<string, object?>> DataTableToDictionary(DataTable dt)
    // { 
    //     var preview = new List<Dictionary<string, object?>>();
    //     foreach (DataRow row in dt.Rows)
    //     {
    //         var rowDict = new Dictionary<string, object?>();
    //         foreach (DataColumn column in dt.Columns)
    //         {
    //             rowDict[column.ColumnName] = row.ItemArray[column.Ordinal];
    //         }
    //         preview.Add(rowDict);
    //     }
    //     return preview;
    // }

    //todo: Determine how to structure preview depending on how UI needs it. This function will be commented out for now

    // private JsonObject DataTableToPreview(DataTable dt)
    // {
    //     var result = new JsonObject();
    //
    //     foreach (DataColumn column in dt.Columns)
    //     {
    //         result[column.ColumnName] = new JsonArray();
    //     }
    //
    //     var maxRows = Math.Min(5, dt.Rows.Count);
    //     for (var i = 0; i < maxRows; i++)
    //     {
    //         var row = dt.Rows[i];
    //         foreach (DataColumn column in dt.Columns)
    //         {
    //             ((JsonArray)result[column.ColumnName]).Add(row[column] == DBNull.Value ? null : row[column]);
    //         }
    //     }
    //
    //     return result;
    // }

    /// <summary>
    /// Converts a data table to csv.
    /// </summary>
    /// <param name="dataTable">A table including the results of the query</param>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The data source ID</param>
    /// <param name="fileName">The name of the file</param>
    /// <exception cref="InvalidOperationException"></exception>
    private void DataTableToCsv(DataTable dataTable, long projectId, long dataSourceId, string fileName)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        StringBuilder sbData = new();

        foreach (var col in dataTable.Columns)
        {
            if (col == null)
                sbData.Append(',');
            else
                sbData.Append("\"" + col.ToString().Replace("\"", "\"\"") + "\",");
        }

        sbData.Replace(",", Environment.NewLine, sbData.Length - 1, 1);

        foreach (DataRow dr in dataTable.Rows)
        {
            foreach (var column in dr.ItemArray)
            {
                if (column == null)
                    sbData.Append(',');
                else
                {
                    string stringColumnValue;
                    if (column is DateTime dateTimeValue)
                    {
                        stringColumnValue = dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    }
                    else
                    {
                        stringColumnValue = $"{column}";
                    }
                    sbData.Append("\"" + stringColumnValue.Replace("\"", "\"\"") + "\",");
                }
            }
            sbData.Replace(",", Environment.NewLine, sbData.Length - 1, 1);
        }

        var filePath = Path.Combine(QueryFolderPath, projectId.ToString(), dataSourceId.ToString(), fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("error creating upload path"));

        File.WriteAllText(filePath, sbData.ToString());
    }

    private static DuckDBConnection GetDuckDbConnection()
    {
        return new DuckDBConnection("Data Source=TimeSeries.db");
    }

    private static DuckDBConnection GetReadOnlyDuckDbConnection()
    {
        return new DuckDBConnection("Data Source=TimeSeries.db;ACCESS_MODE=READ_ONLY");
    }

    /// <summary>
    /// Creates DuckDB table based on the table name and the file path
    /// </summary>
    /// <param name="tableName">Timeseries table name</param>
    /// <param name="filePath">The path of the file being uploaded to DuckDB</param>
    public async Task CreateTimeseriesTable(string tableName, string filePath)
    {
        await using var duckDbConnection = GetDuckDbConnection();
        await duckDbConnection.OpenAsync();

        await using var command = duckDbConnection.CreateCommand();

        command.CommandText = $"CREATE TABLE '{tableName}' AS SELECT * from read_csv('{filePath}', timestampformat = 'TIMESTAMP_NS'); ";
        var executeNonQuery = command.ExecuteNonQuery();
    }

    /// <summary>
    /// Gets all the column names and types from the table
    /// </summary>
    /// <param name="tableName">Timeseries table name</param>
    /// <returns>JSON array of columns</returns>
    private async Task<JsonArray> GetColumnsFromDb(string tableName)
    {
        var columns = new JsonArray();
        await using var duckDbConnection = GetDuckDbConnection();
        await duckDbConnection.OpenAsync();

        await using var command = duckDbConnection.CreateCommand();
        command.CommandText = $"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = '{tableName}';";

        await using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var columnName = reader[0].ToString();
            var columnType = reader[1].ToString();

            var columnObject = new JsonObject
            {
                ["name"] = columnName,
                ["type"] = columnType
            };
            columns.Add(columnObject);
        }
        return columns;
    }

    /// <summary>
    /// Generic select all for given table
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="projectId"></param>
    /// <param name="dataSourceId"></param>
    /// <returns>All data for given table</returns>
    public async Task<RecordResponseDto> GetAllTableRecords(string tableName, long projectId, long dataSourceId)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        var resultTable = new DataTable();
        using var duckDBConnection = GetReadOnlyDuckDbConnection();
        await duckDBConnection.OpenAsync();

        using var command = duckDBConnection.CreateCommand();

        var request = new TimeseriesQueryRequestDto
        {
            Query = $"SELECT * FROM '{tableName}';"
        };

        command.CommandText = request.Query;
        using var reader = command.ExecuteReader();

        resultTable.Load(reader);

        var queryId = Guid.NewGuid().ToString();
        var fileName = queryId + "_record.csv";

        var reportClass = await _classBusiness.GetClassInfo(projectId, "Report");
        var recordRequest = new RecordRequestDto
        {
            Properties = new JsonObject
            {
                ["status"] = Status.InProgress,
                ["query"] = request.Query
            },
            Name = fileName,
            OriginalId = queryId,
            ClassId = reportClass.Id,
            ClassName = reportClass.Name
        };

        var recordResponse = await _recordBusiness.CreateRecord(projectId, dataSourceId, recordRequest);

        RunBackgroundJob(recordResponse, request, resultTable, projectId, dataSourceId, fileName);

        return recordResponse;
    }

    /// <summary>
    /// Queries a table and retrieves every nth row
    /// </summary>
    /// <param name="rowNumber"></param>
    /// <param name="tableName"></param>
    /// <param name="projectId"></param>
    /// <param name="dataSourceId"></param>
    /// <returns>Data</returns>
    public async Task<RecordResponseDto> InterpolateRows(long projectId, long dataSourceId, string rowNumber, string tableName)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        var resultTable = new DataTable();
        using var duckDBConnection = GetReadOnlyDuckDbConnection();
        await duckDBConnection.OpenAsync();
        using var command = duckDBConnection.CreateCommand();

        var request = new TimeseriesQueryRequestDto
        {
            Query = $"""
            
            SELECT * FROM
            (
                SELECT *, ROW_NUMBER() OVER() AS row_num 
                FROM '{tableName}'
            ) AS numbered_table
            WHERE row_num % $rowNum = 0;
            
            """
        };

        command.CommandText = request.Query;
        command.Parameters.Add(new DuckDBParameter("rowNum", long.Parse(rowNumber)));

        using var reader = command.ExecuteReader();
        resultTable.Load(reader);

        var queryId = Guid.NewGuid().ToString();
        var fileName = queryId + "_record.csv";

        var reportClass = await _classBusiness.GetClassInfo(projectId, "Report");
        var recordRequest = new RecordRequestDto
        {
            Properties = new JsonObject
            {
                ["status"] = Status.InProgress,
                ["query"] = request.Query
            },
            Name = fileName,
            OriginalId = queryId,
            ClassId = reportClass.Id,
            ClassName = reportClass.Name
        };

        var recordResponse = await _recordBusiness.CreateRecord(projectId, dataSourceId, recordRequest);

        RunBackgroundJob(recordResponse, request, resultTable, projectId, dataSourceId, fileName);

        return recordResponse;
    }
    
    /// <summary>
    /// Determine if project exists
    /// </summary>
    /// <param name="projectId">The ID of the project we are searching for</param>
    /// <returns>Throws error if project does not exist</returns>
    private void DoesProjectExist(long projectId)
    {
        var project = _context.Projects.Any(p => p.Id == projectId && p.ArchivedAt == null);
        if (!project)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }
    }
    
    /// <summary>
    /// Determine if datasource exists
    /// </summary>
    /// <param name="datasourceId">The ID of the datasource we are searching for</param>
    /// <returns>Throws error if datasource does not exist</returns>
    private void DoesDataSourceExist(long datasourceId)
    {
        var datasource = _context.DataSources.Any(p => p.Id == datasourceId && p.ArchivedAt == null);
        if (!datasource)
        {
            throw new KeyNotFoundException($"Datasource with id {datasourceId} not found");
        }
    }
}
