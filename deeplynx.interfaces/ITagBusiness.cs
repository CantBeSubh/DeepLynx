using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface ITagBusiness
{
    Task<List<TagResponseDto>> GetAllTags(long projectId, bool hideArchived);
    Task<TagResponseDto> GetTag(long projectId, long tagId, bool hideArchived);
    Task<TagResponseDto> CreateTag(long currentUserId, long projectId, CreateTagRequestDto tagRequestDto);
    Task<List<TagResponseDto>> BulkCreateTags(long currentUserId, long projectId, List<CreateTagRequestDto> tags);
    Task<TagResponseDto> UpdateTag(long currentUserId, long projectId, long tagId, UpdateTagRequestDto tagRequestDto);
    Task<bool> DeleteTag(long projectId, long tagId);
    Task<bool> ArchiveTag(long currentUserId, long projectId, long tagId);
    Task<bool> UnarchiveTag(long currentUserId, long projectId, long tagId);
    Task<List<TagResponseDto>> GetTagsByName(long projectId, List<string> tagNames);

}