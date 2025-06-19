using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.interfaces
{
    public interface ITimeseriesBusiness
    {
        Task<RecordResponseDto> UploadFile(string projectId, string dataSourceId, IFormFile file);
        string StartUpload(string projectId, string datasourceId);

        Task<string> UploadChunk(string projectId, string datasourceId, IFormFile chunk,
            string uploadId, int chunkNumber);

        Task<RecordResponseDto> CompleteUpload(string projectId, string datasourceId,
            TimeseriesUploadCompleteRequestDto request);

        Task CreateTimeseriesTable(TimeseriesResponseDto timeseriesResponseDto);
    }
}