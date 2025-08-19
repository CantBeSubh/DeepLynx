using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRecordBusiness
{
    Task<List<RecordResponseDto>> GetAllRecords(
        long projectId, long? dataSourceId, bool hideArchived);
    Task<RecordResponseDto> GetRecord(
        long projectId, long recordId, bool hideArchived);
    Task<RecordResponseDto> CreateRecord(
        long projectId, long dataSourceId, CreateRecordRequestDto dto);
    Task<List<RecordResponseDto>> BulkCreateRecords(
        long projectId, long dataSourceId, List<CreateRecordRequestDto> dtos);
    Task<RecordResponseDto> UpdateRecord(
        long projectId, long recordId, UpdateRecordRequestDto dto);
    Task<bool> DeleteRecord(long projectId, long recordId);
    Task<bool> ArchiveRecord(long projectId, long recordId);
    Task<bool> UnarchiveRecord(long projectId, long recordId);
    Task<bool> AttachTag(long projectId, long recordId, long tagId);
    Task<bool> UnattachTag(long projectId, long recordId, long tagId);
    Task<bool> BulkAttachTags(List<RecordTagLinkDto> dtos);
    Task<List<RecordResponseDto>> GetRecordsByOriginalId(long projectId, List<string> originalIds);

}