using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class TagBusiness : ITagBusiness
{
    private readonly DeeplynxContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for tag operations.</param>
    public TagBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Asynchronously creates a new tag for a specified project.
    /// Note: Will error out with foreign key constraint violation if project is not found.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagRequestDto">The tag request data transfer object containing tag details.</param>
    /// <returns>The created tag response DTO with saved details.</returns>
    public async Task<TagResponseDto> CreateTagAsync(long projectId, TagRequestDto tagRequestDto)
    {
        // Validate 'Name' field
        if (string.IsNullOrWhiteSpace(tagRequestDto.Name))
        {
            throw new ArgumentException("Name is required and cannot be empty or whitespace.");
        }
        
        var tag = new Tag
        {
            Name = tagRequestDto.Name,
            ProjectId = projectId,
            CreatedBy = null, // TODO: handled in future by JWT.
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return new TagResponseDto // Return validated response DTO back to user.
        {
            Id = tag.Id,
            Name = tag.Name,
            ProjectId = tag.ProjectId,
            CreatedBy = tag.CreatedBy,
            CreatedAt = tag.CreatedAt
        };
    }

    /// <summary>
    /// Updates an existing tag for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to update.</param>
    /// <param name="tagRequestDto">The tag request data transfer object containing updated tag details.</param>
    /// <returns>The updated tag response DTO with its details.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
    public async Task<TagResponseDto> UpdateTagAsync(long projectId, long tagId, TagRequestDto tagRequestDto)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId || tag.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Tag with id {tagId} not found");
        }
        
        // Validate 'Name' field
        if (string.IsNullOrWhiteSpace(tagRequestDto.Name))
        {
            throw new ArgumentException("Name is required and cannot be empty.");
        }

        tag.Name = tagRequestDto.Name;
        tag.ModifiedBy = null; // TODO: handled in future by JWT.
        tag.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        await _context.SaveChangesAsync();

        return new TagResponseDto
        {
            Id = tag.Id,
            Name = tag.Name,
            ProjectId = tag.ProjectId,
            CreatedBy = tag.CreatedBy,
            CreatedAt = tag.CreatedAt,
            ModifiedBy = tag.ModifiedBy,
            ModifiedAt = tag.ModifiedAt
        };
    }

    /// <summary>
    /// Retrieves all tags for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project whose tags are to be retrieved.</param>
    /// <returns>A list of tags belonging to the project.</returns>
    public async Task<IEnumerable<TagResponseDto>> GetAllTagsAsync(long projectId)
    {
        return await _context.Tags
            .Where(t => t.ProjectId == projectId && t.DeletedAt == null)
            .Select(t => new TagResponseDto()
            {
                Id = t.Id,
                Name = t.Name,
                ProjectId = t.ProjectId,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
                ModifiedBy = t.ModifiedBy,
                ModifiedAt = t.ModifiedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to retrieve.</param>
    /// <returns>The tag with its details.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
    public async Task<TagResponseDto> GetTagByIdAsync(long projectId, long tagId)
    {
        var tag = await _context.Tags
            .Where(t => t.ProjectId == projectId && t.Id == tagId && t.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (tag == null)
        {
            throw new KeyNotFoundException("Tag not found.");
        }

        return new TagResponseDto
        {
            Id = tag.Id,
            Name = tag.Name,
            ProjectId = tag.ProjectId,
            CreatedBy = tag.CreatedBy,
            CreatedAt = tag.CreatedAt,
            ModifiedBy = tag.ModifiedBy,
            ModifiedAt = tag.ModifiedAt
        };
    }

    /// <summary>
    /// Deletes a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <param name="force">Indicates whether to force delete the tag if it is in use.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
    public async Task<bool> DeleteTagAsync(long projectId, long tagId, bool force = false)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId || tag.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Tag with {tagId} not found.");
        }

        if (force)
        {
            _context.Tags.Remove(tag);
        }
        else
        {
            tag.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.Tags.Update(tag);
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
}