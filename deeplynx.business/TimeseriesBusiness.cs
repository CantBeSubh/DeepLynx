using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.business;

public class TimeseriesBusiness(DeeplynxContext context) : ITimeseriesBusiness
{
    private readonly DeeplynxContext _context = context;
    private const string UploadFolderPath = "uploads";

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

        // todo: kick off file processing here
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

    public string StartUpload(string projectId, string dataSourceId)
    {
        var uploadId = Guid.NewGuid().ToString();
        var folderPath = Path.Combine(UploadFolderPath, projectId, dataSourceId, uploadId);
        Directory.CreateDirectory(folderPath);

        return uploadId;
    }

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

        return "Chunk uploaded successfully.";
    }

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
        
        // todo: kick off file processing here
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