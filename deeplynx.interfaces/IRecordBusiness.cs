using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;


namespace deeplynx.interfaces;

public interface IRecordBusiness
{
    Task<IEnumerable<RecordResponseDto>> GetAllRecords(long projectId, long dataSourceId);
    Task<RecordResponseDto> GetRecord(long projectId, long dataSourceId, long recordId);
    Task<RecordResponseDto> CreateRecord(long projectId, long dataSourceId, RecordRequestDto dto);
    Task<RecordResponseDto> UpdateRecord(long projectId, long dataSourceId, long recordId, RecordRequestDto dto);
    Task<bool> DeleteRecord(long projectId, long recordId, bool force);
    Task<bool> BulkSoftDeleteRecords(string domainType, long domainId, IDbContextTransaction? transaction);
}