using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface ITagBusiness
{
    Task<TagResponseDto> CreateTag(long projectId, TagRequestDto tagRequestDto);
    Task<BulkTagResponseDto> BulkCreateTags(long projectId, BulkTagRequestDto bulkTagRequestDto);
    Task<TagResponseDto> UpdateTag(long projectId, long tagId, TagRequestDto tagRequestDto);
    Task<IEnumerable<TagResponseDto>> GetAllTags(long projectId, bool hideArchived);
    Task<TagResponseDto> GetTagById(long projectId, long tagId, bool hideArchived);
    Task<bool> DeleteTag(long projectId, long tagId);
    Task<bool> ArchiveTag(long projectId, long tagId);
}