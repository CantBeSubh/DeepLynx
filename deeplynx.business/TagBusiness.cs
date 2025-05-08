using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class TagBusiness : ITagBusiness
{
    private readonly DeeplynxContext _context;

    public TagBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<TagRequestDto> CreateTagAsync(long projectId, TagRequestDto TagRequestDto)
    {
        var tag = new Tag
        {
            Name = TagRequestDto.Name,
            ProjectId = projectId,
            CreatedBy = TagRequestDto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        TagRequestDto.Id = tag.Id;
        TagRequestDto.CreatedAt = tag.CreatedAt;

        return TagRequestDto;
    }

    public async Task<TagRequestDto> UpdateTagAsync(long projectId, long tagId, TagRequestDto TagRequestDto)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId)
        {
            throw new KeyNotFoundException("Tag not found.");
        }

        tag.Name = TagRequestDto.Name;
        tag.ModifiedBy = TagRequestDto.ModifiedBy;
        tag.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TagRequestDto.ModifiedAt = tag.ModifiedAt;

        return TagRequestDto;
    }

    public async Task<IEnumerable<TagRequestDto>> GetAllTagsAsync(long projectId)
    {
        return await _context.Tags
            .Where(t => t.ProjectId == projectId && t.DeletedAt == null)
            .Select(t => new TagRequestDto
            {
                Id = t.Id,
                Name = t.Name,
                ProjectId = t.ProjectId,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
                ModifiedBy = t.ModifiedBy,
                ModifiedAt = t.ModifiedAt,
                DeletedAt = t.DeletedAt
            })
            .ToListAsync();
    }

    public async Task<TagRequestDto> GetTagByIdAsync(long projectId, long tagId)
    {
        var tag = await _context.Tags
            .Where(t => t.ProjectId == projectId && t.Id == tagId && t.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (tag == null)
        {
            throw new KeyNotFoundException("Tag not found.");
        }

        return new TagRequestDto
        {
            Id = tag.Id,
            Name = tag.Name,
            ProjectId = tag.ProjectId,
            CreatedBy = tag.CreatedBy,
            CreatedAt = tag.CreatedAt,
            ModifiedBy = tag.ModifiedBy,
            ModifiedAt = tag.ModifiedAt,
            DeletedAt = tag.DeletedAt
        };
    }

    public async Task DeleteTagAsync(long projectId, long tagId, bool force = false)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId)
        {
            throw new KeyNotFoundException("Tag not found.");
        }

        if (force)
        {
            _context.Tags.Remove(tag);
        }
        else
        {
            tag.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}