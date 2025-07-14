using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;


namespace deeplynx.interfaces;

public interface IRecordBusiness
{
    Task<IEnumerable<RecordResponseDto>> GetAllRecords(long projectId, long? dataSourceId, bool hideArchived);
    Task<RecordResponseDto> GetRecord(long projectId, long recordId, bool hideArchived);
    Task<RecordResponseDto> CreateRecord(long projectId, long dataSourceId, RecordRequestDto dto);
    Task<RecordResponseDto> UpdateRecord(long projectId, long recordId, RecordRequestDto dto);
    Task<bool> DeleteRecord(long projectId, long recordId);
    Task<bool> ArchiveRecord(long projectId, long recordId);
}