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
    
    /// <summary>
    /// Uploads a time series file and kicks off the processing for DuckDB
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="dataSourceId">The Data Source ID</param>
    /// <param name="file">This is the entire file attached as form data in the request</param>
    /// <returns>An object of the uploaded file information</returns>
    /// <exception cref="ArgumentException">If the file is null or has no data</exception>
    /// <exception cref="InvalidOperationException">If the server cannot create the directory</exception>
    public async Task<TimeseriesResponseDto> UploadFile(string projectId, string dataSourceId, IFormFile file)
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
        
        ClassResponseDto recordClass = await GetClassInfo(projectId);
        var columns = await GetColumnsFromDb(tableName);
            
        var recordRequest = new RecordRequestDto 
        {
            Properties = new JsonObject
            {
                ["columns"] = columns,
                ["timeUploaded"] = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ["fileType"] = Path.GetExtension(file.FileName).TrimStart('.').ToLower()
            },
            Name = file.FileName,
            OriginalId = uploadId,
            Uri = uri,
            ClassId = recordClass.Id,
            ClassName = recordClass.Name,
        };
        
        await _recordBusiness.CreateRecord(long.Parse(projectId), long.Parse(dataSourceId), recordRequest);
        
        return new TimeseriesResponseDto
        {
            ProjectId = projectId,
            DataSourceId = dataSourceId,
            FileId = uploadId,
            FileName = file.FileName,
            FilePath = filePath,
            FileType = Path.GetExtension(file.FileName).TrimStart('.').ToLower()
        };
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
    public async Task<TimeseriesResponseDto> CompleteUpload(string projectId, string dataSourceId,
        TimeseriesUploadCompleteRequestDto request)
    {
        var folderPath = Path.Combine(UploadFolderPath, projectId, dataSourceId, request.UploadId);
        string tableName = request.UploadId + "_" + request.FileName;
        var finalFilePath = Path.Combine(UploadFolderPath, projectId, dataSourceId,
            request.UploadId + "_" + request.FileName);
        var uri = "duckdb://" + request.UploadId + "_" + request.FileName; 

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
        // "duckdb://uuid_filename"

        ClassResponseDto recordClass = await GetClassInfo(projectId);

        var columns = await GetColumnsFromDb(tableName);
            
        var recordRequest = new RecordRequestDto 
        {
            Properties = new JsonObject
            {
                ["columns"] = columns,
                ["timeUploaded"] = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ["fileType"] = Path.GetExtension(request.FileName).TrimStart('.').ToLower()
            },
            Name = request.FileName,
            OriginalId = request.UploadId,
            Uri = uri,
            ClassId = recordClass.Id,
            ClassName = recordClass.Name
        };
        
        await _recordBusiness.CreateRecord(long.Parse(projectId), long.Parse(dataSourceId), recordRequest);
        
        return new TimeseriesResponseDto
        {
            ProjectId = projectId,
            DataSourceId = dataSourceId,
            FileId = request.UploadId,
            FileName = request.FileName,
            FilePath = finalFilePath,
            FileType = Path.GetExtension(request.FileName).TrimStart('.').ToLower()
        };
    }
    private async Task<JsonArray> GetColumnsFromDb(string tableName)
    {
        await using var duckDbConnection = new DuckDBConnection("Data Source=file.db");
        duckDbConnection.Open();

        await using var command = duckDbConnection.CreateCommand();
        command.CommandText = "SELECT column_name, data_type " +
                              "FROM information_schema.columns " +
                              "WHERE table_name = $table_name;";
        command.Parameters.Add(new DuckDBParameter("table_name", tableName));

        await using var reader = command.ExecuteReader();
        var columns = new JsonArray();
        
        while (reader.Read())
        {
            var columnName = reader["COLUMN_NAME"].ToString();
            var columnType = reader["DATA_TYPE"].ToString();
            
            var columnObject = new JsonObject
            {
                ["name"] = columnName,
                ["type"] = columnType
            };

            columns.Add(columnObject);
        }
        return columns;
    }

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