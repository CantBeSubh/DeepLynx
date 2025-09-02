using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.business;

public class FileS3Business:  IFileBusiness
{
    public async Task<RecordResponseDto> UploadFile(long projectId, long datasourceId, long objectStorageId,
        IFormFile file)
    {
        return new RecordResponseDto();
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