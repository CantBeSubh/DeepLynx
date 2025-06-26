using System.Data;
using System.Text.Json.Nodes;
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

        Task CreateTimeseriesTable(string tableName, string filePath);

        Task<List<Dictionary<string, object?>>> QueryTimeseries(TimeseriesQueryRequestDto request, string projectId, string dataSourceId);
        Task CreateTimeseriesTable(TimeseriesResponseDto timeseriesResponseDto);

        Task<List<List<dynamic>>> QueryEveryNRows(int rowNumber, string tableName);

        Task<List<List<dynamic>>> GetAllTableRecords(string tableName);

        Task<List<List<dynamic>>> RawQueryTimeseries(string query);
    }
}