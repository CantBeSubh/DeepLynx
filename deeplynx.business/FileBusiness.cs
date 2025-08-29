using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace deeplynx.business;

public class FileBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IFileBusinessFactory _factory;
    private readonly IObjectStorageBusiness _objectStorageBusiness;
    private readonly IClassBusiness _classBusiness;
    private readonly IRecordBusiness _recordBusiness;

    public FileBusiness(
        DeeplynxContext context, 
        IFileBusinessFactory factory, 
        IObjectStorageBusiness objectStorageBusiness, 
        IClassBusiness classBusiness, 
        IRecordBusiness recordBusiness)
    {
        _context = context;
        _factory = factory;
        _objectStorageBusiness = objectStorageBusiness;
        _classBusiness = classBusiness;
        _recordBusiness = recordBusiness;
    }
    public async Task<RecordResponseDto> UploadFile( 
        long projectId,
        long? dataSourceId,
        long? objectStorageId,
        IFormFile file)
    {
        // TODO: check for default data source and use that and throw an error if not found
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        await ExistenceHelper.EnsureDataSourceExistsAsync(_context, dataSourceId.Value);
        ObjectStorage? objectStorage;
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required and cannot be empty.");
        }
        
        if (objectStorageId is not null)
        {
            objectStorage = await _context.ObjectStorages.FirstOrDefaultAsync(
                     os=> os.Id == objectStorageId 
                     && os.ProjectId == projectId 
                     && os.ArchivedAt != null
                     );
        }
        else
        {
            var defaultObjectStorageResponseDto = await _objectStorageBusiness.GetDefaultObjectStorage(projectId);
            objectStorage = await _context.ObjectStorages.FindAsync(defaultObjectStorageResponseDto.Id);
        }

        if (objectStorage is null)
        {
            throw new InvalidOperationException("No object storage found for project");
        }
        
        // Check config to confirm it is valid (could be part of the object storage fetch)
        var configData = JsonConvert.DeserializeObject<ObjectStorageConfigDto>(objectStorage.Config);
        if (configData == null)
        {
            throw new InvalidOperationException($"Config data for object storage is null");
        }
        
        var fileBusiness = _factory.CreateFileBusiness(objectStorage.Type);
        
        var filePath = await fileBusiness.UploadFile(projectId, dataSourceId.Value, configData, file);
        
        var fileClass = await _classBusiness.GetClassInfo(projectId, "File");
        var recordRequest = new CreateRecordRequestDto
        {
            Properties = new JsonObject()
            {
                ["fileType"] = Path.GetExtension(file.FileName).TrimStart('.').ToLower()
            },
            Name = file.FileName,
            ObjectStorageId = objectStorage.Id,
            Description = file.FileName,
            OriginalId = Guid.NewGuid().ToString(),
            Uri = filePath,
            ClassId = fileClass.Id,
            ClassName = fileClass.Name,
        };
        
        // return the newly created metadata record for the file
        return await _recordBusiness.CreateRecord(projectId, dataSourceId.Value, recordRequest);
    }

    public async Task<RecordResponseDto> UpdateFile(string storageType, long projectId, long datasourceId, long objectStorageId, long recordId, IFormFile file)
    {
        var fileBusiness = _factory.CreateFileBusiness(storageType);
        return await fileBusiness.UpdateFile(projectId, datasourceId, objectStorageId, recordId, file);
    }

    public async Task<FileStreamResult> DownloadFile(string storageType, long projectId, long datasourceId, long objectStorageId, long recordId)
    {
        var fileBusiness = _factory.CreateFileBusiness(storageType);
        return await fileBusiness.DownloadFile(projectId, datasourceId, objectStorageId, recordId);
    }

    public async Task<bool> DeleteFile(string storageType, long projectId, long objectStorageId, long recordId)
    {
        var fileBusiness = _factory.CreateFileBusiness(storageType);
        return await fileBusiness.DeleteFile(projectId, objectStorageId, recordId);
    }
}