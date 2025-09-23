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
    private readonly ICacheBusiness _cacheBusiness;
    private readonly IFileBusinessFactory _factory;
    private readonly IObjectStorageBusiness _objectStorageBusiness;
    private readonly IDataSourceBusiness _dataSourceBusiness;
    private readonly IClassBusiness _classBusiness;
    private readonly IRecordBusiness _recordBusiness;

    public FileBusiness(
        DeeplynxContext context,
        ICacheBusiness cacheBusiness,
        IFileBusinessFactory factory,
        IObjectStorageBusiness objectStorageBusiness,
        IDataSourceBusiness dataSourceBusiness,
        IClassBusiness classBusiness,
        IRecordBusiness recordBusiness)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
        _factory = factory;
        _objectStorageBusiness = objectStorageBusiness;
        _dataSourceBusiness = dataSourceBusiness;
        _classBusiness = classBusiness;
        _recordBusiness = recordBusiness;
    }

    /// <summary>
    /// Uploads file using specified object storage method
    /// </summary>
    /// <param name="projectId">Id of the project to which the file belongs</param>
    /// <param name="dataSourceId">Id of the data source to which the file belongs</param>
    /// <param name="objectStorageId">Id of the object storage method to use</param>
    /// <param name="file">file to upload</param>
    public async Task<RecordResponseDto> UploadFile(
        long projectId,
        long? dataSourceId,
        long? objectStorageId,
        IFormFile file)
    {
        long realDataSourceId;
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required and cannot be empty.");
        }
        if (dataSourceId.HasValue)
        {
            await ExistenceHelper.EnsureDataSourceExistsAsync(_context, dataSourceId.Value);
            realDataSourceId = dataSourceId.Value;
        }
        else
        {
            var defaultDataSource = await _dataSourceBusiness.GetDefaultDataSource(projectId) ?? throw new KeyNotFoundException("Default data source not found");
            realDataSourceId = defaultDataSource.Id;
        }

        ObjectStorage? objectStorage;

        if (objectStorageId is not null)
        {
            objectStorage = await _context.ObjectStorages.FirstOrDefaultAsync(
                     os => os.Id == objectStorageId
                     && os.ProjectId == projectId
                     && !os.IsArchived
                     );
        }
        else
        {
            var defaultObjectStorageResponseDto = await _objectStorageBusiness.GetDefaultObjectStorage(projectId);
            objectStorage = await _context.ObjectStorages.FindAsync(defaultObjectStorageResponseDto.Id);
        }

        if (objectStorage is null)
        {
            throw new KeyNotFoundException("No object storage found for project");
        }

        // Check config to confirm it is valid (could be part of the object storage fetch)
        var configData = JsonConvert.DeserializeObject<ObjectStorageConfigDto>(objectStorage.Config);
        if (configData == null)
        {
            throw new InvalidOperationException($"Config data for object storage is null");
        }

        var fileBusiness = _factory.CreateFileBusiness(objectStorage.Type);

        var guid = Guid.NewGuid();

        var uri = await fileBusiness.UploadFile(projectId, realDataSourceId, configData, file, guid);

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
            OriginalId = guid.ToString(),
            Uri = uri,
            ClassId = fileClass.Id,
            ClassName = fileClass.Name,
        };

        // return the newly created metadata record for the file
        return await _recordBusiness.CreateRecord(projectId, realDataSourceId, recordRequest);
    }

    /// <summary>
    /// Relaces a file but uses the same guid for the file name
    /// </summary>
    /// <param name="projectId">Id of the project to which the file belongs</param>
    /// <param name="recordId">Id of record that contains the info of the file to replace</param>
    /// <param name="file">file to update to</param>
    public async Task<RecordResponseDto> UpdateFile(long projectId, long recordId, IFormFile file)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var record = await _recordBusiness.GetRecord(projectId, recordId, true);
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required and cannot be empty.");
        }

        if (record.ObjectStorageId == null)
        {
            throw new KeyNotFoundException("Record needs an object storage id");
        }

        var objectStorage = await _objectStorageBusiness.GetObjectStorage(projectId, record.ObjectStorageId.Value, true);

        var fileBusiness = _factory.CreateFileBusiness(objectStorage.Type);

        var uri = await fileBusiness.UpdateFile(record, file);

        var updateRecordRequest = new UpdateRecordRequestDto
        {
            Properties = new JsonObject()
            {
                ["fileType"] = Path.GetExtension(file.FileName).TrimStart('.').ToLower(),
            },
            Name = file.FileName,
            Uri = uri,

        };
        return await _recordBusiness.UpdateRecord(projectId, recordId, updateRecordRequest);
    }

    /// <summary>
    /// Downloads file
    /// </summary>
    /// <param name="projectId">Id of the project to which the file belongs</param>
    /// <param name="recordId">Id of record that contains the info of the file to download</param>
    public async Task<FileStreamResult> DownloadFile(long projectId, long recordId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var record = await _recordBusiness.GetRecord(projectId, recordId, true);
        if (record.ObjectStorageId == null)
        {
            throw new KeyNotFoundException("Record needs an object storage id");
        }
        var objectStorage = await _objectStorageBusiness.GetObjectStorage(projectId, record.ObjectStorageId.Value, true);
        var fileBusiness = _factory.CreateFileBusiness(objectStorage.Type);
        return await fileBusiness.DownloadFile(record);
    }

    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="projectId">Id of the project to which the file belongs</param>
    /// <param name="recordId">Id of record that contains the info of the file to delete</param>
    public async Task<bool> DeleteFile(long projectId, long recordId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var record = await _recordBusiness.GetRecord(projectId, recordId, true);
        if (record == null)
        {
            throw new KeyNotFoundException("Record not found");
        }
        if (record.ObjectStorageId == null)
        {
            throw new KeyNotFoundException("Record needs an object storage id");
        }
        var objectStorage = await _objectStorageBusiness.GetObjectStorage(projectId, record.ObjectStorageId.Value, true);
        var fileBusiness = _factory.CreateFileBusiness(objectStorage.Type);
        await fileBusiness.DeleteFile(record);
        return await _recordBusiness.DeleteRecord(projectId, recordId);

    }
}