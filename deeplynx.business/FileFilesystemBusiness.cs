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
        // TODO: check for default obj storage / datasource and don't require them
        // TODO: Cache these
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        await ExistenceHelper.EnsureDataSourceExistsAsync(_context, dataSourceId);
        var objectStorage = await _context.ObjectStorages.FindAsync(objectStorageId);
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required and cannot be empty.");
        }
        
        // Check config to confirm it is valid (could be part of the object storage fetch)
        var configData = JsonConvert.DeserializeObject<ObjectStorageConfigDto>(objectStorage.Config);
        if (configData == null || configData.MountPath == null)
        {
            throw new Exception("Config data is null or invalid.");
        }
        
        // create a file path in the format <mountdir>/project_<id>/datasource_<id>/filename
        var filePath = Path.Combine(
            configData.MountPath, 
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

        // retrieve or insert the File class to assign it to the new record
        // TODO: record creation + return could be moved to FileBusiness
        var fileClass = await _classBusiness.GetClassInfo(projectId, "File");
        var recordRequest = new CreateRecordRequestDto
        {
            Properties = new JsonObject
            {
                ["fileType"] = Path.GetExtension(file.FileName).TrimStart('.').ToLower()
            },
            Name = file.FileName,
            Description = file.FileName,
            OriginalId = Guid.NewGuid().ToString(),
            Uri = filePath,
            ClassId = fileClass.Id,
            ClassName = fileClass.Name,
        };

        // return the newly created metadata record for the file
        // TODO: set object storage ID
        return await _recordBusiness.CreateRecord(projectId, dataSourceId, recordRequest);
    }
    // TODO: Upload File
    // TODO: Update File (upload a newer copy)
    // TODO: Download File
    // TODO: Delete File
}