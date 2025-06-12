using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.interfaces
{
    public interface ITimeseriesBusiness
    {
        Task<TimeseriesResponseDto> UploadFile(string projectId, string dataSourceId, IFormFile file);
        string StartUpload(string projectId, string datasourceId);

        Task<string> UploadChunk(string projectId, string datasourceId, IFormFile chunk,
            string uploadId, int chunkNumber);

        Task<TimeseriesResponseDto> CompleteUpload(string projectId, string datasourceId,
            TimeseriesUploadCompleteRequestDto request);

        Task<List<List<dynamic>>> GetAllTableRecords(TimeseriesResponseDto timeseriesResponseDto);

        Task ProcessTimeSeriesDataAsync(TimeseriesResponseDto timeseriesResponseDto);
    }
}