using deeplynx.models;

namespace deeplynx.interfaces;
    
public interface ITagBusiness
{
    Task<TagResponseDto> CreateTagAsync(long projectId, TagRequestDto tagRequestDto);
    Task<TagResponseDto> UpdateTagAsync(long projectId, long tagId, TagRequestDto tagRequestDto);
    Task<IEnumerable<TagResponseDto>> GetAllTagsAsync(long projectId);
    Task<TagResponseDto> GetTagByIdAsync(long projectId, long tagId);
    Task<bool> DeleteTagAsync(long projectId, long tagId, bool force = false);
    Task<bool> BulkSoftDeleteTags(string domainType, long domainId);
}