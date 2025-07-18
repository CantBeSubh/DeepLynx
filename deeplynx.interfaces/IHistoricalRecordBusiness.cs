using deeplynx.models;

namespace deeplynx.interfaces;

public interface IHistoricalRecordBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> GetAllHistoricalRecords(
        long projectId, long? dataSourceId, DateTime? pointInTime, 
        bool hideArchived, bool current);
    Task<IEnumerable<HistoricalRecordResponseDto>> GetHistoryForRecord(long recordId);
    Task<HistoricalRecordResponseDto> GetHistoricalRecord(
        long recordId, DateTime? pointInTime, bool hideArchived, bool current);
}