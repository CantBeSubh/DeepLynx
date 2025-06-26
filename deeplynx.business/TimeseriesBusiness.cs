using System.Data;
using System.Text;
using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using DuckDB.NET.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class TimeseriesBusiness(DeeplynxContext context, IRecordBusiness recordBusiness, IClassBusiness classBusiness) : ITimeseriesBusiness
{
    private readonly DeeplynxContext _context = context;
    private readonly IRecordBusiness _recordBusiness = recordBusiness;
    private readonly IClassBusiness _classBusiness = classBusiness;
    private const string UploadFolderPath = "uploads";
    private const string QueryFolderPath = "reports";
    
    /// <summary>
    /// Uploads a time series file and kicks off the processing for DuckDB
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The Data Source ID</param>
    /// <param name="file">This is the entire file attached as form data in the request</param>
    /// <returns>An object of the uploaded file information</returns>
    /// <exception cref="ArgumentException">If the file is null or has no data</exception>
    /// <exception cref="InvalidOperationException">If the server cannot create the directory</exception>
    public async Task<RecordResponseDto> UploadFile(string projectId, string dataSourceId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required and cannot be empty or whitespace.");
        }

        var uploadId = Guid.NewGuid().ToString();
        string tableName = uploadId + "_" + file.FileName;
        var filePath = Path.Combine(UploadFolderPath, projectId, dataSourceId, uploadId + "_" + file.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("error creating upload path"));
        var uri = "duckdb://" + tableName;

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // todo: kick off file processing here (See DL-97 Sub-Tasks)
        // start saving metadata to db
        // import into duckdb
        // describe table for metadata record properties
        // after processing, new filepath should be something like
        // "duckdb://path/to/uuid_filename"

        await CreateTimeseriesTable(tableName, filePath);
        
        var recordClass = await GetClassInfo(projectId);
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

        return await _recordBusiness.CreateRecord(long.Parse(projectId), long.Parse(dataSourceId), recordRequest);
    }

    /// <summary>
    /// Sets up the directory for file chunks to be uploaded
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The Data Source ID</param>
    /// <returns>The upload ID (guid format) for file chunks to go to the right directory</returns>
    public string StartUpload(string projectId, string dataSourceId)
    {
        var uploadId = Guid.NewGuid().ToString();
        var folderPath = Path.Combine(UploadFolderPath, projectId, dataSourceId, uploadId);
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
    public async Task<string> UploadChunk(string projectId, string dataSourceId, IFormFile chunk,
        string uploadId, int chunkNumber)
    {
        if (chunk == null || chunk.Length == 0)
        {
            throw new ArgumentException("No chunk uploaded.");
        }

        var tempFilePath = Path.Combine(UploadFolderPath, projectId, dataSourceId, uploadId, $"{chunkNumber}.part");
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
    public async Task<RecordResponseDto> CompleteUpload(string projectId, string dataSourceId,
        TimeseriesUploadCompleteRequestDto request)
    {
        var folderPath = Path.Combine(UploadFolderPath, projectId, dataSourceId, request.UploadId);
        var tableName = request.UploadId + "_" + request.FileName;
        var finalFilePath = Path.Combine(UploadFolderPath, projectId, dataSourceId,
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
        
        var recordClass = await GetClassInfo(projectId);
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

        return await _recordBusiness.CreateRecord(long.Parse(projectId), long.Parse(dataSourceId), recordRequest);
    }
    
    /// <summary>
    /// This allows the user to query timeseries data in duckDb. Creates the command using an sql string
    /// The connection is read only so any write operations will be blocked.
    /// </summary>
    /// <param name="request"> The request which includes the query string</param>
    /// <param name="projectId"></param>
    /// <param name="dataSourceId"></param>
    /// <returns></returns>
    public async Task<List<Dictionary<string, object?>>> QueryTimeseries(TimeseriesQueryRequestDto request, string projectId, string dataSourceId)
    {
        var resultTable = new DataTable();
        await using var duckDbConnection = GetReadOnlyDuckDbConnection();
        await duckDbConnection.OpenAsync();
        
        await using var command = duckDbConnection.CreateCommand();
        command.CommandText = request.Query;
        await using var reader = await command.ExecuteReaderAsync();
        
        // return empty list if no rows returned
        // todo: add a report/record with a status of something like "Empty response" 
        if (!reader.HasRows)
        {
            var noResultList = new List<Dictionary<string, object?>>
            {
                new()
                {
                    { "status", "no rows matching query" }
                }
            };

            return noResultList;
        }
        resultTable.Load(reader);
        
        // Runs in the background and lets the request finish
        // https://stackoverflow.com/questions/62222712/what-is-the-simplest-way-to-run-a-single-background-task-from-a-controller-in-n
        // todo: modify task to work with db connections. Need to create the tasks own scope as shown in the 2nd
        // example in the stack overflow conversation above.
        // Write report record to postgres
        // Write csv to object storage
        Task.Run(() =>
        { 
            DataTableToCsv(resultTable, projectId, dataSourceId);
        });
        var result = DataTableToDictionary(resultTable);
        
        return result;
    }
    
    /// <summary>
    /// Makes a JSON like response from the table. This will be replaced by a link to the csv later.
    /// </summary>
    /// <param name="dt"> data table with response data</param>
    /// <returns></returns>
    private List<Dictionary<string, object?>> DataTableToDictionary(DataTable dt)
    { 
        var result = new List<Dictionary<string, object?>>();
        foreach (DataRow row in dt.Rows)
        {
            var rowDict = new Dictionary<string, object?>();
            foreach (DataColumn column in dt.Columns)
            {
                rowDict[column.ColumnName] = row.ItemArray[column.Ordinal];
            }
            result.Add(rowDict);
        }
        return result;
    }
    
    /// <summary>
    /// Converts a data table to csv.
    /// </summary>
    /// <param name="dataTable">A table including the results of the query</param>
    /// <param name="projectId"></param>
    /// <param name="dataSourceId"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void DataTableToCsv(DataTable dataTable, string projectId, string dataSourceId) {
        StringBuilder sbData = new StringBuilder();
        
        foreach (var col in dataTable.Columns) {
            if (col == null)
                sbData.Append(",");
            else
                sbData.Append("\"" + col.ToString().Replace("\"", "\"\"") + "\",");
        }

        sbData.Replace(",", Environment.NewLine, sbData.Length - 1, 1);

        foreach (DataRow dr in dataTable.Rows) {
            foreach (var column in dr.ItemArray) {
                if (column == null)
                    sbData.Append(",");
                else
                {
                    string stringColumnValue;
                    if (column is DateTime dateTimeValue) {
                        stringColumnValue = dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    } else {
                        stringColumnValue = $"{column}";
                    }
                    sbData.Append("\"" + stringColumnValue.Replace("\"", "\"\"") + "\",");
                }   
            }
            sbData.Replace(",", Environment.NewLine, sbData.Length - 1, 1);
        }
        
        var queryId = Guid.NewGuid().ToString();
        var filePath = Path.Combine(QueryFolderPath, projectId, dataSourceId, queryId + "_" + "report.csv");
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
    /// <returns></returns>
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
    /// <returns></returns>
    private async Task<JsonArray> GetColumnsFromDb(string tableName)
    {
        var columns = new JsonArray();
        await using var duckDBConnection = GetDuckDbConnection();
        await duckDBConnection.OpenAsync();

        await using var command = duckDBConnection.CreateCommand();
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
    /// Gets the timeseries class in the project that we are setting the record in.
    /// Creates a timeseries class if there is not one already
    /// </summary>
    /// <param name="projectId">The ID of the project we are searching</param>
    /// <returns></returns>
    private async Task<ClassResponseDto> GetClassInfo(string projectId)
    {
        var timeseriesClass = await _context.Classes.FirstOrDefaultAsync(c => c.Name == "Timeseries" && c.ProjectId == long.Parse(projectId));

        if (timeseriesClass != null)
        {
            return new ClassResponseDto()
            {
                Id = timeseriesClass.Id,
                Name = timeseriesClass.Name,
            };
        }
        
        var classDto = new ClassRequestDto()
        {
            Name = "Timeseries"
        };

        return await _classBusiness.CreateClass(long.Parse(projectId), classDto);
    }
}