using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.business;

public class FileBusiness
{
    private readonly IFileBusinessFactory _factory;

    public FileBusiness(IFileBusinessFactory factory)
    {
        _factory = factory;
    }
    public async Task<RecordResponseDto> UploadFile(string storageType, long projectId, long datasourceId, long objectStorageId, IFormFile file)
    {
        var fileBusiness = _factory.CreateFileBusiness(storageType);
        return await fileBusiness.UploadFile(projectId, datasourceId, objectStorageId, file);
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