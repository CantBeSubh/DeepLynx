using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface IRecordMappingBusiness
{
    Task<IEnumerable<RecordMappingResponseDto>> GetAllRecordMappings(long projectId, long? classId, long? tagId);
    Task<RecordMappingResponseDto> GetRecordMapping(long projectId, long mappingId);
    Task<RecordMappingResponseDto> CreateRecordMapping(long projectId, RecordMappingRequestDto dto);
    Task<RecordMappingResponseDto> UpdateRecordMapping(long projectId, long mappingId, RecordMappingRequestDto dto);
    Task<bool> DeleteRecordMapping(long projectId, long mappingId);
    Task<bool> ArchiveRecordMapping(long projectId, long mappingId);
}