using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using deeplynx.helpers;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.StaticFiles;

namespace deeplynx.business;

public class FileFilesystemBusiness : IFileBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IObjectStorageBusiness _objectStorageBusiness;
    private readonly IClassBusiness _classBusiness;
    private readonly IRecordBusiness _recordBusiness;

    public FileFilesystemBusiness(
        DeeplynxContext context, 
        IObjectStorageBusiness objectStorageBusiness,
        IClassBusiness classBusiness,
        IRecordBusiness recordBusiness)
    {
        _context = context;
        _objectStorageBusiness = objectStorageBusiness;
        _classBusiness = classBusiness;
        _recordBusiness = recordBusiness;
    }
    public async Task<string> UploadFile(
        long projectId,
        long dataSourceId,
        ObjectStorageConfigDto objectStorageConfig,
        IFormFile file,
        Guid guid
    )
    {
        // TODO: check for default obj storage / datasource and don't require them
        // TODO: Cache these
        if (objectStorageConfig.MountPath == null)
        {
            throw new Exception("File system mount path not set in object storage");
        }
        
        var fileName = $"{guid}_{file.FileName}";
        
        // create a file path in the format <mountdir>/project_<id>/datasource_<id>/filename
        var filePath = Path.Combine(
            objectStorageConfig.MountPath, 
            "project_" + projectId.ToString(),
            "datasource_" + dataSourceId.ToString(),
            fileName);
        // create the directory for the file if not exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("error creating upload path."));

        // copy the file to its new location
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        // TODO: set object storage ID
        return filePath;
    }
    
    public async Task<string> UpdateFile(RecordResponseDto record, IFormFile file)
    {
        var filePath = record.Uri;
        
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is not specified in the record.");
        }
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The file to update does not exist.", filePath);
        }
        
        string? directory = Path.GetDirectoryName(filePath);

        if (directory == null || !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException("Directory not found.");
        }

        var newFileName = $"{record.OriginalId}_{file.FileName}";
        
        var updatedPath = Path.Combine(directory, newFileName);
        
        // Delete the original file
        File.Delete(filePath);
        
        //write new file
        await using (var stream = new FileStream(updatedPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        return updatedPath;
    }

    public async Task<FileStreamResult> DownloadFile(RecordResponseDto record)
    {
        var filePath = record.Uri;
        var fileName = record.Name;
        
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must be provided.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The requested file does not exist.", filePath);
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        
        // Detect file type
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
        {
            contentType = "application/octet-stream"; // Default fallback
        }
        
        return new FileStreamResult(stream, contentType)
        {
            FileDownloadName = fileName
        };

    }

    public async Task<bool> DeleteFile(RecordResponseDto record)
    {
        var filePath = record.Uri;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is not specified in the record.");
        }
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The file to update does not exist.", filePath);
        }
        
        File.Delete(filePath);
        return true;
    }
}