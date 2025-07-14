using deeplynx.models;

namespace deeplynx.interfaces;

public interface IHistoricalRecordBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> GetAllHistoricalRecords(
        long projectId, long? dataSourceId, DateTime? pointInTime, bool hideArchived, bool current);
    Task<IEnumerable<HistoricalRecordResponseDto>> GetHistoryForRecord(long recordId);
    Task<HistoricalRecordResponseDto> GetHistoricalRecord(long recordId, DateTime? pointInTime, bool current);
    Task<bool> CreateHistoricalRecord(long recordId);
    Task<bool> UpdateHistoricalRecord(long recordId);
    Task<bool> ArchiveHistoricalRecord(long recordId);
    Task<bool> DeleteHistoricalRecord(long recordId);
}