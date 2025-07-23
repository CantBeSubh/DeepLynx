using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRecordBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> GetAllRecords(long projectId, long? dataSourceId, bool hideArchived);
    Task<HistoricalRecordResponseDto> GetRecord(long projectId, long recordId, bool hideArchived);
    Task<RecordResponseDto> CreateRecord(long projectId, long dataSourceId, RecordRequestDto dto);
    Task<List<RecordResponseDto>> BulkCreateRecords(long projectId, long dataSourceId, List<RecordRequestDto> recordRequestDtos);
    Task<RecordResponseDto> UpdateRecord(long projectId, long recordId, RecordRequestDto dto);
    Task<bool> DeleteRecord(long projectId, long recordId);
    Task<bool> ArchiveRecord(long projectId, long recordId);
    Task<bool> UnarchiveRecord(long projectId, long recordId);
}