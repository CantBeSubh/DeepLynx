using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.exceptions;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.business;

public class TagBusiness : ITagBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IRecordMappingBusiness _recordMappingBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for tag operations.</param>
    public TagBusiness(DeeplynxContext context, IRecordMappingBusiness recordMappingBusiness)
    {
        _context = context;
        _recordMappingBusiness = recordMappingBusiness;
    }

    /// <summary>
    /// Asynchronously creates a new tag for a specified project.
    /// Note: Will error out with foreign key constraint violation if project is not found.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagRequestDto">The tag request data transfer object containing tag details.</param>
    /// <returns>The created tag response DTO with saved details.</returns>
    public async Task<TagResponseDto> CreateTag(long projectId, TagRequestDto tagRequestDto)
    {
        DoesProjectExist(projectId);
        if (tagRequestDto == null)
            throw new ArgumentNullException(nameof(tagRequestDto));
        
        
        var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Name == tagRequestDto.Name);
        if (existingTag != null)
        {
            throw new Exception($"Tag for project {projectId} with name {tagRequestDto.Name} already exists");
        }
        
        // Validate 'Name' field
        if (string.IsNullOrWhiteSpace(tagRequestDto.Name))
        {
            throw new ValidationException("Name is required and cannot be empty or whitespace");
        }
        
        /*// Validate 'CreatedBy' field
        if (string.IsNullOrWhiteSpace(tagRequestDto.CreatedBy))
        {
            throw new ValidationException("CreatedBy is required and cannot be empty or whitespace");
        }*/
        
        var tag = new Tag
        {
            Name = tagRequestDto.Name,
            ProjectId = projectId,
            CreatedBy = !String.IsNullOrEmpty(tagRequestDto.CreatedBy) ? tagRequestDto.CreatedBy : null, // TODO: handled in future by JWT.
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        await _context.Tags.AddAsync(tag);
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
    /// Asynchronously creates new tags for a specified project.
    /// Note: Will error out with foreign key constraint violation if project is not found.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagRequestDto">The tag request data transfer object containing tag details.</param>
    /// <returns>The created tag response DTO with saved details.</returns>
    public async Task<BulkTagResponseDto> BulkCreateTags(long projectId, BulkTagRequestDto bulkTagRequestDto)
    {
        DoesProjectExist(projectId);
        ValidationHelper.ValidateModel(bulkTagRequestDto);
        
        var tags = new List<Tag>();
        var tagResponses = new List<TagResponseDto>();
        foreach (var tagRequestDto in bulkTagRequestDto.Tags)
        {
            ValidationHelper.ValidateModel(tagRequestDto);
            var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Name == tagRequestDto.Name);
            if (existingTag != null)
            {
                throw new Exception($"Tag for project {projectId} with name {tagRequestDto.Name} already exists");
            }
            
            var tag = new Tag
            {
                Name = tagRequestDto.Name,
                ProjectId = projectId,
                CreatedBy = !String.IsNullOrEmpty(tagRequestDto.CreatedBy) ? tagRequestDto.CreatedBy : null,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            tags.Add(tag);
        }
        await _context.Tags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();

        foreach (var tag in tags)
        {
            var tagResponseDto = new TagResponseDto
            {
                Id = tag.Id,
                Name = tag.Name,
                ProjectId = tag.ProjectId,
                CreatedBy = tag.CreatedBy,
                CreatedAt = tag.CreatedAt
            };
            tagResponses.Add(tagResponseDto);
        }
        
        return new BulkTagResponseDto
        {
            Tags = tagResponses
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
    public async Task<TagResponseDto> UpdateTag(long projectId, long tagId, TagRequestDto tagRequestDto)
    {
        DoesProjectExist(projectId);
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId || tag.ArchivedAt is not null)
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
    /// <param name="hideArchived">Flag indicating whether to hide archived tags from the result</param>
    /// <returns>A list of tags belonging to the project.</returns>
    public async Task<IEnumerable<TagResponseDto>> GetAllTags(long projectId, bool hideArchived)
    {
        DoesProjectExist(projectId, hideArchived);
        var tagQuery = _context.Tags
            .Where(t => t.ProjectId == projectId);
            
        if (hideArchived)
        {
            tagQuery = tagQuery.Where(t => t.ArchivedAt == null);
        }
            
        return await tagQuery.Select(t => new TagResponseDto()
            {
                Id = t.Id,
                Name = t.Name,
                ProjectId = t.ProjectId,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
                ModifiedBy = t.ModifiedBy,
                ModifiedAt = t.ModifiedAt,
                ArchivedAt = t.ArchivedAt,
            })
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to retrieve.</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived tags from the result</param>
    /// <returns>The tag with its details.</returns>
    /// <exception cref="KeyNotFoundException">Returned if tag not found or is archived</exception>
    public async Task<TagResponseDto> GetTagById(long projectId, long tagId, bool hideArchived)
    {
        DoesProjectExist(projectId, hideArchived);
        var tag = await _context.Tags
            .Where(t => t.ProjectId == projectId && t.Id == tagId)
            .FirstOrDefaultAsync();

        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with id {tagId} not found");
        }
        
        if (hideArchived && tag.ArchivedAt != null)
        {
            throw new KeyNotFoundException($"Tag with id {tagId} is archived");
        }

        return new TagResponseDto
        {
            Id = tag.Id,
            Name = tag.Name,
            ProjectId = tag.ProjectId,
            CreatedBy = tag.CreatedBy,
            CreatedAt = tag.CreatedAt,
            ModifiedBy = tag.ModifiedBy,
            ModifiedAt = tag.ModifiedAt,
            ArchivedAt = tag.ArchivedAt,
        };
    }

    /// <summary>
    /// Deletes a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
    public async Task<bool> DeleteTag(long projectId, long tagId)
    {
        DoesProjectExist(projectId);
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId)
            throw new KeyNotFoundException($"Tag with id {tagId} not found.");
        
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Archives a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to archive.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
    public async Task<bool> ArchiveTag(long projectId, long tagId)
    {
        DoesProjectExist(projectId);
        var tag = await _context.Tags.FindAsync(tagId);

        if (tag == null || tag.ProjectId != projectId || tag.ArchivedAt is not null)
            throw new KeyNotFoundException($"Tag with id {tagId} not found");

        tag.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Unarchives a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to unarchive.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
    public async Task<bool> UnarchiveTag(long projectId
        , long tagId)
    {
        DoesProjectExist(projectId);
        var tag = await _context.Tags.FindAsync(tagId);

        if (tag == null || tag.ProjectId != projectId || tag.ArchivedAt is null)
            throw new KeyNotFoundException($"Tag with id {tagId} not found or is not archived.");

        tag.ArchivedAt = null;
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Determine if project exists
    /// </summary>
    /// <param name="projectId">The ID of the project we are searching for</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
    /// <returns>Throws error if project does not exist</returns>
    private void DoesProjectExist(long projectId, bool hideArchived = true)
    {
        var project = hideArchived ? _context.Projects.Any(p => p.Id == projectId && p.ArchivedAt == null) 
            : _context.Projects.Any(p => p.Id == projectId);
        if (!project)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }
    }
    
}