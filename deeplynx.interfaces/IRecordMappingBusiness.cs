using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRecordMappingBusiness
{
    Task<IEnumerable<RecordMappingResponseDto>> GetAllRecordMappings(long projectId, long? classId, long? tagId);
    Task<RecordMappingResponseDto> GetRecordMapping(long mappingId, long projectId);
    Task<RecordMappingResponseDto> CreateRecordMapping(long projectId, RecordMappingRequestDto dto);
    Task<RecordMappingResponseDto> UpdateRecordMapping(long projectId, long mappingId, RecordMappingRequestDto dto);
    Task<bool> DeleteRecordMapping(long mappingId, long projectId, bool force);
    Task<bool> SoftDeleteAllRecordMappingsByProjectIdAsync(long projectId);
}