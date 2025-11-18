using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRecordBusiness
{
    Task<List<RecordResponseDto>> GetAllRecords(
        long projectId, long? dataSourceId, bool hideArchived, string? fileType);
    Task<List<RecordResponseDto>> GetRecordsByTags(
        long projectId, long[] tagIds, bool hideArchived);
    Task<RecordResponseDto> GetRecord(
        long projectId, long recordId, bool hideArchived);
    Task<RecordResponseDto> CreateRecord(
        long currentUserId, long projectId, long dataSourceId, CreateRecordRequestDto dto);
    Task<List<RecordResponseDto>> BulkCreateRecords(
        long currentUserId, long projectId, long dataSourceId, List<CreateRecordRequestDto> dtos);
    Task<RecordResponseDto> UpdateRecord(
        long currentUserId, long projectId, long recordId, UpdateRecordRequestDto dto);
    Task<bool> DeleteRecord(long projectId, long recordId);
    Task<bool> ArchiveRecord(long currentUserId, long projectId, long recordId);
    Task<bool> UnarchiveRecord(long currentUserId, long projectId, long recordId);
    Task<bool> AttachTag(long projectId, long recordId, long tagId);
    Task<bool> UnattachTag(long projectId, long recordId, long tagId);
    Task<bool> BulkAttachTags(List<RecordTagLinkDto> dtos);
    Task<List<RecordResponseDto>> GetRecordsByOriginalId(long projectId, List<string> originalIds);

}