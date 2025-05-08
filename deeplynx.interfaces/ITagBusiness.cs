using deeplynx.models;

namespace deeplynx.interfaces;
    
public interface ITagBusiness
{
    Task<TagRequestDto> CreateTagAsync(long projectId, TagRequestDto TagRequestDto);
    Task<TagRequestDto> UpdateTagAsync(long projectId, long tagId, TagRequestDto TagRequestDto);
    Task<IEnumerable<TagRequestDto>> GetAllTagsAsync(long projectId);
    Task<TagRequestDto> GetTagByIdAsync(long projectId, long tagId);
    Task DeleteTagAsync(long projectId, long tagId, bool force = false);
}