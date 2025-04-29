using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRecordBusiness
{
    Task<IEnumerable<Record>> GetAllRecords(long projectId, long dataSourceId);
    Task<Record> GetRecord(long projectId, long dataSourceId, long recordId);
    Task<Record> CreateRecord(long projectId, long dataSourceId, RecordRequestDto dto);
    Task<Record> UpdateRecord(long projectId, long dataSourceId, long recordId, RecordRequestDto dto);
    Task<bool> DeleteRecord(long projectId, long dataSourceId, long recordId);
    int CalculateJsonMaxDepth(JsonNode node); 
}