using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRecordMappingBusiness
{
    Task<IEnumerable<RecordMapping>> GetAllRecordMappings(long projectId);
    Task<RecordMapping> GetRecordMapping(long mappingId);
    Task<RecordMapping> CreateRecordMapping(long projectId, RecordMappingRequestDto dto);
    Task<RecordMapping> UpdateRecordMapping(long projectId, long mappingId, RecordMappingRequestDto dto);
    Task<bool> DeleteRecordMapping(long mappingId);
    Task<bool> SoftDeleteAllRecordMappingsByProjectIdAsync(long projectId);
}