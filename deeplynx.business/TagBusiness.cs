using System.ComponentModel.DataAnnotations;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using deeplynx.helpers;
using deeplynx.helpers.exceptions;

namespace deeplynx.business;

public class TagBusiness : ITagBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IRecordMappingBusiness _recordMappingBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for tag operations.</param>
    /// <param name="recordMappingBusiness">Passed in context of record mapping objects</param>
    public TagBusiness(DeeplynxContext context, IRecordMappingBusiness recordMappingBusiness)
    {
        _context = context;
        _recordMappingBusiness = recordMappingBusiness;
    }
    
        /// <summary>
    /// Retrieves all tags for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project whose tags are to be retrieved.</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived tags from the result</param>
    /// <returns>A list of tags belonging to the project.</returns>
    public async Task<List<TagResponseDto>> GetAllTags(long projectId, bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
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
    public async Task<TagResponseDto> GetTag(long projectId, long tagId, bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
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
    /// Asynchronously creates a new tag for a specified project.
    /// Note: Will error out with foreign key constraint violation if project is not found.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="dto">The tag request data transfer object containing tag details.</param>
    /// <returns>The created tag response DTO with saved details.</returns>
    public async Task<TagResponseDto> CreateTag(long projectId, CreateTagRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        
        
        var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Name == dto.Name);
        if (existingTag != null)
        {
            throw new Exception($"Tag for project {projectId} with name {dto.Name} already exists");
        }
        
        // Validate 'Name' field
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException("Name is required and cannot be empty or whitespace");
        }
        
        /*// Validate 'CreatedBy' field
        if (string.IsNullOrWhiteSpace(dto.CreatedBy))
        {
            throw new ValidationException("CreatedBy is required and cannot be empty or whitespace");
        }*/
        
        var tag = new Tag
        {
            Name = dto.Name,
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
    /// Asynchronously creates new tags for a specified project.
    /// Note: Will error out with foreign key constraint violation if project is not found.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tags">The tag request data transfer object containing tag details.</param>
    /// <returns>The created tag response DTO with saved details.</returns>
    public async Task<List<TagResponseDto>> BulkCreateTags(
        long projectId, 
        List<CreateTagRequestDto> tags)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        
        // Bulk insert into classes; if there is a name collision, update the description and uuid if present
        var sql = @"
            INSERT INTO deeplynx.tags (project_id, name, created_at)
            VALUES {0}
            ON CONFLICT (project_id, name) DO UPDATE SET
                modified_at = @now
            RETURNING *;
        ";

        // establish "constant" parameters
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@projectId", projectId),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
        };
        
        // establish "dynamic" parameters (new for each dto in the list)
        parameters.AddRange(tags.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@p{i}_name", dto.Name)
        }));
        
        // stringify the params and comma separate them
        var valueTuples = string.Join(", ", tags.Select((dto, i) =>
            $"(@projectId, @p{i}_name, @now)"));
        
        // put everything together and execute the query
        sql = string.Format(sql, valueTuples);

        // returns the resulting upserted classes
        return await _context.Database
            .SqlQueryRaw<TagResponseDto>(sql, parameters.ToArray())
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing tag for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to update.</param>
    /// <param name="tagRequestDto">The tag request data transfer object containing updated tag details.</param>
    /// <returns>The updated tag response DTO with its details.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
    public async Task<TagResponseDto> UpdateTag(long projectId, long tagId, UpdateTagRequestDto tagRequestDto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
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

        tag.Name = tagRequestDto.Name ?? tag.Name;
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
    /// Deletes a specific tag by its ID for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the tag belongs.</param>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the tag is not found.</exception>
    public async Task<bool> DeleteTag(long projectId, long tagId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var tag = await _context.Tags.FindAsync(tagId);

        if (tag == null || tag.ProjectId != projectId || tag.ArchivedAt is null)
            throw new KeyNotFoundException($"Tag with id {tagId} not found or is not archived.");

        tag.ArchivedAt = null;
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
        return true;
    }
    
}