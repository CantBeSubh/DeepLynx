using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.interfaces
{
    public interface ITimeseriesBusiness
    {
        Task<RecordResponseDto> UploadFile(long projectId, long datasourceId, IFormFile file);
        Task<string> StartUpload(long projectId, long datasourceId);

        Task<string> UploadChunk(long projectId, long datasourceId, IFormFile chunk,
            string uploadId, int chunkNumber);

        Task<RecordResponseDto> CompleteUpload(long projectId, long datasourceId,
            TimeseriesUploadCompleteRequestDto request);

        Task CreateTimeseriesTable(long projectId, long dataSourceId, string tableName, string filePath);

        Task<RecordResponseDto> QueryTimeseries(TimeseriesQueryRequestDto request, long projectId, long datasourceId, string fileType);

        Task<RecordResponseDto> InterpolateRows(long projectId, long datasourceId, string rowNumber, string tableName, string fileType);

        Task<RecordResponseDto> ExportTimeseriesTable(long projectId, long datasourceId, string tableName, string fileType);
    }
}