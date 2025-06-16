using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.business;

public class TimeseriesBusiness(DeeplynxContext context) : ITimeseriesBusiness
{
    private readonly DeeplynxContext _context = context;
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
        var filePath = Path.Combine(UploadFolderPath, projectId, dataSourceId, uploadId + "_" + file.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("error creating upload path"));

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
        var finalFilePath = Path.Combine(UploadFolderPath, projectId, dataSourceId, request.UploadId + "_" + request.FileName);

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

    public async Task<List<List<dynamic>>> GetAllTableRecords(TimeseriesResponseDto timeseriesResponseDto)
    {
        using DuckDBConnection duckDBConnection = GetDuckDBConnection();
        await duckDBConnection.OpenAsync();
        using DuckDBCommand command = duckDBConnection.CreateCommand();

        command.CommandText = "SELECT * FROM $table_name";
        command.Parameters.Add(new DuckDBParameter("table_name", timeseriesResponseDto.FileName));
        using DuckDBDataReader reader = command.ExecuteReader();

        return OrganizeQueryData(reader);
    }

    public async Task ProcessTimeSeriesDataAsync(TimeseriesResponseDto timeseriesResponseDto)
    {
        using DuckDBConnection duckDBConnection = GetDuckDBConnection();
        await duckDBConnection.OpenAsync();
        //Transaction? duckDBConnection.BeginTransaction();

        using DuckDBCommand command = duckDBConnection.CreateCommand();
        int executeNonQuery;

        //TODO What to do if table already exists
        command.CommandText = "CREATE TABLE $file_name AS SELECT * from '$file_name';"; //https://duckdb.org/docs/stable/data/overview.html
        command.Parameters.Add(new DuckDBParameter("$file_name", timeseriesResponseDto.FilePath));
        executeNonQuery = command.ExecuteNonQuery();
    }
}
