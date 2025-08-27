using deeplynx.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.interfaces;

public interface IFileBusiness
{
    Task<RecordResponseDto> UploadFile(long projectId, long dataSourceId, long objectStorageId, IFormFile file);
    // Task<RecordResponseDto> UpdateFile(long projectId, long dataSourceId, long objectStorageId, long recordId, IFormFile file);
    // Task<FileStreamResult> DownloadFile(long projectId, long dataSourceId, long objectStorageId, long recordId);
    // Task<bool>  DeleteFile(long projectId, long objectStorageId, long recordId);
}