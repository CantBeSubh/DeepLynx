using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using deeplynx.helpers;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        IFormFile file
    )
    {
        // TODO: check for default obj storage / datasource and don't require them
        // TODO: Cache these
        if (objectStorageConfig.MountPath == null)
        {
            throw new Exception("File system mount path not set in object storage");
        }
        
        // create a file path in the format <mountdir>/project_<id>/datasource_<id>/filename
        var filePath = Path.Combine(
            objectStorageConfig.MountPath, 
            "project_" + projectId.ToString(),
            "datasource_" + dataSourceId.ToString(),
            file.FileName);
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
    
    public async Task<RecordResponseDto> UpdateFile(long projectId, long datasourceId, long objectStorageId,
        long recordId, IFormFile file)
    {
        return new RecordResponseDto();
    }

    public async Task<FileStreamResult> DownloadFile(long projectId, long datasourceId, long objectStorageId,
        long recordId)
    {
        // Create a simple stub with empty content
        var emptyStream = new MemoryStream();
        return new FileStreamResult(emptyStream, "application/octet-stream")
        {
            FileDownloadName = "stub-file.txt"
        };
    }

    public async Task<bool> DeleteFile(long projectId, long objectStorageId, long recordId)
    {
        return true;
    }
}