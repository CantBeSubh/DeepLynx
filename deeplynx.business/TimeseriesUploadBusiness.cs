using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.business;

public class TimeseriesUploadBusiness : ITimeseriesUploadBusiness
{
    private readonly DeeplynxContext _context;
    private const string UploadFolderPath = "uploads";

    public TimeseriesUploadBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<TimeseriesResponseDto> UploadFile(string projectId, string dataSourceId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required and cannot be empty or whitespace.");
        }

        var uploadId = Guid.NewGuid().ToString();
        var filePath = Path.Combine(UploadFolderPath, uploadId, "_", file.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // todo: kick off file processing here
        // after processing, new filepath should be like
        // "duckdb://path/to/uuid-filename"
        
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
    
    // todo: convert timeseriesBusiness to timeseriesQueryBusiness
}