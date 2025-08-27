using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using deeplynx.helpers;
using deeplynx.interfaces;
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
    public async Task<RecordResponseDto> UploadFile(
        long projectId,
        long dataSourceId,
        long objectStorageId,
        IFormFile file
    )
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        await ExistenceHelper.EnsureDataSourceExistsAsync(_context, dataSourceId);
        var objectStorage = await _context.ObjectStorages.FindAsync(objectStorageId);
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required and cannot be empty.");
        }
        
        var configData = JsonConvert.DeserializeObject<ObjectStorageConfigDto>(objectStorage.Config);
        if (configData == null || configData.MountPath == null)
        {
            throw new Exception("Config data is null or invalid.");
        }
        
        var filePath = Path.Combine(
            configData.MountPath, 
            "project_" + projectId.ToString(),
            "datasource_" + dataSourceId.ToString(),
            file.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("error creating upload path."));

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var recordClass = await _classBusiness.GetClassInfo(projectId, "File");
        var recordRequest = new CreateRecordRequestDto
        {
            Properties = new JsonObject
            {
                ["fileType"] = file.ContentType
            },
            Name = file.FileName,
            Description = file.FileName,
            OriginalId = Guid.NewGuid().ToString(),
            Uri = filePath,
            ClassId = recordClass.Id,
            ClassName = recordClass.Name,
        };

        return await _recordBusiness.CreateRecord(projectId, dataSourceId, recordRequest);
    }
    // TODO: Upload File
    // TODO: Update File (upload a newer copy)
    // TODO: Download File
    // TODO: Delete File
}