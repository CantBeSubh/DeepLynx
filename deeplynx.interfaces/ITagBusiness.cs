using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;
    
public interface ITagBusiness
{
    Task<TagResponseDto> CreateTag(long projectId, TagRequestDto tagRequestDto);
    Task<TagResponseDto> UpdateTag(long projectId, long tagId, TagRequestDto tagRequestDto);
    Task<IEnumerable<TagResponseDto>> GetAllTags(long projectId);
    Task<TagResponseDto> GetTagById(long projectId, long tagId);
    Task<bool> DeleteTag(long projectId, long tagId, bool force = false);
    Task<bool> BulkSoftDeleteTags(Expression<Func<Tag, bool>> predicate, IDbContextTransaction? transaction);
}