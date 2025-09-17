using System.ComponentModel.DataAnnotations;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using deeplynx.helpers;
using deeplynx.helpers.exceptions;
using System.Text.Json;

namespace deeplynx.business;

public class TagBusiness : ITagBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;
    private readonly ICacheBusiness _cacheBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for tag operations.</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    /// <param name="eventBusiness">Used to access event operations</param>
    public TagBusiness(DeeplynxContext context, ICacheBusiness cacheBusiness, IEventBusiness eventBusiness)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
        _eventBusiness = eventBusiness;
    }
    
    /// <summary>
    /// Retrieves all tags for a specified project.
    /// </summary>
    /// <param name="projectIds">The IDs of the projects whose tags are to be retrieved.</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived tags from the result</param>
    /// <returns>A list of tags belonging to the project.</returns>
    public async Task<List<TagResponseDto>> GetAllTags(long[] projectIds, bool hideArchived)
    {
        foreach (var projectId in projectIds)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);
        }
        var tagQuery = _context.Tags
            .Where(t => projectIds.Contains(t.ProjectId));
            
        if (hideArchived)
        {
            tagQuery = tagQuery.Where(t => !t.IsArchived);
        }
            
        return await tagQuery.Select(t => new TagResponseDto()
            {
                Id = t.Id,
                Name = t.Name,
                ProjectId = t.ProjectId,
                LastUpdatedBy = t.LastUpdatedBy,
                LastUpdatedAt = t.LastUpdatedAt,
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);
        var tag = await _context.Tags
            .Where(t => t.ProjectId == projectId && t.Id == tagId)
            .FirstOrDefaultAsync();

        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with id {tagId} not found");
        }
        
        if (hideArchived && tag.IsArchived)
        {
            throw new KeyNotFoundException($"Tag with id {tagId} is archived");
        }

        return new TagResponseDto
        {
            Id = tag.Id,
            Name = tag.Name,
            ProjectId = tag.ProjectId,
            LastUpdatedBy = tag.LastUpdatedBy,
            LastUpdatedAt = tag.LastUpdatedAt,
            IsArchived = tag.IsArchived,
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
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
            LastUpdatedBy = null, // TODO: handled in future by JWT.
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        
        // Log Tag Create Event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = tag.ProjectId,
            Operation = "create",
            EntityType = "tag",
            EntityId = tag.Id,
            Properties = JsonSerializer.Serialize(new {tag.Name}),
            DataSourceId = null,
            LastUpdatedBy = "" // TODO: add username when JWT are implemented
        });

        return new TagResponseDto // Return validated response DTO back to user.
        {
            Id = tag.Id,
            Name = tag.Name,
            ProjectId = tag.ProjectId,
            LastUpdatedBy = tag.LastUpdatedBy,
            LastUpdatedAt = tag.LastUpdatedAt
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        
        // Bulk insert into classes; if there is a name collision, update the description and uuid if present
        var sql = @"
            INSERT INTO deeplynx.tags (project_id, name, last_updated_at, is_archived, last_updated_by)
            VALUES {0}
            ON CONFLICT (project_id, name) DO UPDATE SET
                last_updated_at = @now
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
            $"(@projectId, @p{i}_name, @now, false, NUll)"));
        
        // put everything together and execute the query
        sql = string.Format(sql, valueTuples);

        // returns the resulting upserted classes
        var result = await _context.Database
            .SqlQueryRaw<TagResponseDto>(sql, parameters.ToArray())
            .ToListAsync();
        
        // log tag create event for each tag created
        var events = new List<CreateEventRequestDto>{ };
        foreach (var item in result)
        {
            events.Add(new CreateEventRequestDto
            {
                ProjectId = item.ProjectId,
                Operation = "create",
                EntityType = "tag",
                EntityId = item.Id,
                Properties = JsonSerializer.Serialize(new {item.Name}),
                DataSourceId = null,
                LastUpdatedBy = "" // TODO: add username when JWT are implemented
            });
        }
        await _eventBusiness.BulkCreateEvents(projectId, events);
        
        return result;
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId || tag.IsArchived)
        {
            throw new KeyNotFoundException($"Tag with id {tagId} not found");
        }
        
        // Validate 'Name' field
        if (string.IsNullOrWhiteSpace(tagRequestDto.Name))
        {
            throw new ArgumentException("Name is required and cannot be empty.");
        }

        tag.Name = tagRequestDto.Name ?? tag.Name;
        tag.LastUpdatedBy= null; // TODO: handled in future by JWT.
        tag.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
        
        // Log tag update event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            Operation = "update",
            EntityType = "tag",
            EntityId = tag.Id,
            ProjectId = tag.ProjectId,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new {tag.Name}),
            LastUpdatedBy = "" // TODO: add username when JWT are implemented
        });

        return new TagResponseDto
        {
            Id = tag.Id,
            Name = tag.Name,
            ProjectId = tag.ProjectId,
            LastUpdatedBy = tag.LastUpdatedBy,
            LastUpdatedAt = tag.LastUpdatedAt,
            IsArchived = tag.IsArchived,
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var tag = await _context.Tags.FindAsync(tagId);

        if (tag == null || tag.ProjectId != projectId || tag.IsArchived)
            throw new KeyNotFoundException($"Tag with id {tagId} not found");

        tag.IsArchived = true;
        tag.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
        
        // Log tag soft delete event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            Operation = "delete",
            EntityType = "tag",
            EntityId = tag.Id,
            ProjectId = tag.ProjectId,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new {tag.Name}),
            LastUpdatedBy = "" // TODO: add username when JWT are implemented
        });
        
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var tag = await _context.Tags.FindAsync(tagId);

        if (tag == null || tag.ProjectId != projectId || !tag.IsArchived)
            throw new KeyNotFoundException($"Tag with id {tagId} not found or is not archived.");

        tag.IsArchived = false;
        tag.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Validates that all provided tag names exist in the database for the specified project.
    /// Used by MetadataBusiness to enforce tagsMutable settings.
    /// </summary>
    /// <param name="projectId">The project ID to search within</param>
    /// <param name="tagNames">List of tag names to validate</param>
    /// <returns>List of tags that were found</returns>
    /// <exception cref="KeyNotFoundException">Thrown if one or more tag names not found</exception>
    /// <exception cref="ArgumentException">Thrown if tagNames list is null or empty</exception>
    public async Task<List<TagResponseDto>> GetTagsByName(long projectId, List<string> tagNames)
    {
        if (tagNames == null || !tagNames.Any())
            throw new ArgumentException("Tag names list cannot be null or empty", nameof(tagNames));

        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);

        // Remove duplicates and filter out null/empty values
        var cleanTagNames = tagNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList();

        if (!cleanTagNames.Any())
            throw new ArgumentException("No valid tag names provided", nameof(tagNames));

        // Query for existing tags (excluding archived)
        var existingTags = await _context.Tags
            .Where(t => t.ProjectId == projectId 
                        && !t.IsArchived
                        && cleanTagNames.Contains(t.Name))
            .ToListAsync();
    
        var foundTagNames = existingTags.Select(t => t.Name).ToHashSet();
        var missingTagNames = cleanTagNames.Where(name => !foundTagNames.Contains(name)).ToList();

        if (missingTagNames.Any())
        {
            throw new KeyNotFoundException(
                $"Tags not found with names: {string.Join(", ", missingTagNames)}");
        }
    
        return existingTags.Select(t => new TagResponseDto
        {
            Id = t.Id,
            Name = t.Name,
            ProjectId = t.ProjectId,
            LastUpdatedBy = t.LastUpdatedBy,
            LastUpdatedAt = t.LastUpdatedAt,
            IsArchived = t.IsArchived,
        }).ToList();
    }
}