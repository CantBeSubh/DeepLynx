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
    /// <param name="tagRequestDto">The tag data transfer object containing tag details.</param>
    /// <returns>The created tag with its details.</returns>
    public async Task<TagRequestDto> CreateTagAsync(long projectId, TagRequestDto TagRequestDto)
    {
        // Validate 'Name' and 'CreatedBy' fields
        if (string.IsNullOrWhiteSpace(TagRequestDto.Name))
        {
            throw new ArgumentException("Name is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(TagRequestDto.CreatedBy))
        {
            throw new ArgumentException("CreatedBy is required and cannot be empty or whitespace.");
        }
        var tag = new Tag
        {
            Name = TagRequestDto.Name,
            ProjectId = projectId, 
            CreatedBy = TagRequestDto.CreatedBy,
            ModifiedBy = TagRequestDto.CreatedBy,
            CreatedAt = DateTime.Now, // NOW: saves without time zones
            ModifiedAt = DateTime.Now 
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        TagRequestDto.Id = tag.Id;
        TagRequestDto.CreatedAt = tag.CreatedAt;

        return TagRequestDto; // Return validated DTO back to user.
    }

    /// <summary>
    /// Updates an existing tag for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to update.</param>
    /// <param name="TagRequestDto">The tag data transfer object containing updated tag details.</param>
    /// <returns>The updated tag with its details.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
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

    /// <summary>
    /// Retrieves all tags for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project whose tags are to be retrieved.</param>
    /// <returns>A list of tags belonging to the project.</returns>
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

    /// <summary>
    /// Retrieves a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to retrieve.</param>
    /// <returns>The tag with its details.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
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

    /// <summary>
    /// Deletes a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <param name="force">Indicates whether to force delete the tag if it is in use.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
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