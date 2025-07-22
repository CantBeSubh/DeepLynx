using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRecordBusiness
{
    Task<IEnumerable<RecordResponseDto>> GetAllRecords(
        long projectId, long? dataSourceId, bool hideArchived);
    Task<RecordResponseDto> GetRecord(
        long projectId, long recordId, bool hideArchived);
    Task<RecordResponseDto> CreateRecord(
        long projectId, long dataSourceId, RecordRequestDto dto);
    Task<BulkRecordResponseDto> BulkCreateRecords(
        long projectId, long dataSourceId, BulkRecordRequestDto dto);
    Task<RecordResponseDto> UpdateRecord(
        long projectId, long recordId, RecordRequestDto dto);
    Task<bool> DeleteRecord(long projectId, long recordId);
    Task<bool> ArchiveRecord(long projectId, long recordId);
    Task<bool> UnarchiveRecord(long projectId, long recordId);
}