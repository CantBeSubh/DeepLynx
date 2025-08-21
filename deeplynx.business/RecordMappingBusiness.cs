using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using deeplynx.helpers;
using deeplynx.helpers.exceptions;
using System.Text.Json;

namespace deeplynx.business;
//todo:
//1. Add dto validator for all methods that use dto
//2. Check to see if project exists when used in endpoint
public class RecordMappingBusiness : IRecordMappingBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordMappingBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the record mapping operations.</param>
    public RecordMappingBusiness(DeeplynxContext context,  IEventBusiness eventBusiness)
    {
        _context = context;
        _eventBusiness = eventBusiness;
    }

    /// <summary>
    /// Retrieves all record mappings for a specific project and (optionally) class or tag.
    /// </summary>
    /// <param name="projectId">The ID of the project whose mappings are to be retrieved</param>
    /// <param name="classId">(Optional) the ID of the class by which to filter mappings</param>
    /// <param name="tagId">(Optional) the ID of the tag by which to filter mappings</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived mappings from the result</param>
    /// <returns>A list of record mappings based on the applied filters.</returns>
    /// TODO: Handle return message for if no records exist 
    public async Task<IEnumerable<RecordMappingResponseDto>> GetAllRecordMappings(
        long projectId,
        long? classId,
        long? tagId,
        bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId,  hideArchived);
        var mappingQuery = _context.RecordMappings
            .Where(m => m.ProjectId == projectId);

        // add filter for class or tag if specified
        if (classId.HasValue)
        {
            var rmClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId && c.ArchivedAt == null);
            if (rmClass == null)
            {
                throw new KeyNotFoundException($"Class with id {classId} not found");
            }
            mappingQuery = mappingQuery.Where(m => m.ClassId == classId);
        }

        if (tagId.HasValue)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(p => p.Id == tagId && p.ArchivedAt == null);
            if (tag == null)
            {
                throw new KeyNotFoundException($"Tag with id {tagId} not found");
            }
            mappingQuery = mappingQuery.Where(m => m.TagId == tagId);
        }
        
        if (hideArchived)
        {
            mappingQuery = mappingQuery.Where(m => m.ArchivedAt == null);
        }
            
        var mappings = await mappingQuery.ToListAsync();
            
        return mappings
            .Select(m => new RecordMappingResponseDto()
            {
                Id = m.Id,
                RecordParams = JsonNode.Parse(m.RecordParams) as JsonObject,
                ClassId = m.ClassId,
                ProjectId = m.ProjectId,
                TagId = m.TagId,
                DataSourceId = m.DataSourceId,
                CreatedBy = m.CreatedBy,
                CreatedAt = m.CreatedAt,
                ModifiedBy = m.ModifiedBy,
                ModifiedAt = m.ModifiedAt,
                ArchivedAt = m.ArchivedAt,
            })
            .ToList();
    }

    /// <summary>
    /// Retrieves a specific mapping by its id
    /// </summary>
    /// <param name="projectId">The project ID for the project to which the mapping belongs</param>
    /// <param name="mappingId">The id whereby to fetch the mapping</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived mappings from the result</param>
    /// <returns>The mapping associated with the given ID</returns>
    /// <exception cref="KeyNotFoundException">Returned if mapping not found or is archived</exception>
    public async Task<RecordMappingResponseDto> GetRecordMapping(
        long projectId,
        long mappingId,
        bool hideArchived
        )
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
        
        var mapping = await _context.RecordMappings
            .Where(m => m.Id == mappingId && m.ProjectId == projectId)
            .FirstOrDefaultAsync();

        if (mapping == null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} not found");
        }
        
        if (hideArchived && mapping.ArchivedAt != null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} is archived");
        }

        return new RecordMappingResponseDto
        {
            Id = mapping.Id,
            RecordParams = JsonNode.Parse(mapping.RecordParams) as JsonObject,
            ClassId = mapping.ClassId,
            ProjectId = mapping.ProjectId,
            TagId = mapping.TagId,
            DataSourceId = mapping.DataSourceId,
            CreatedBy = mapping.CreatedBy,
            CreatedAt = mapping.CreatedAt,
            ModifiedBy = mapping.ModifiedBy,
            ModifiedAt = mapping.ModifiedAt,
            ArchivedAt = mapping.ArchivedAt,
        };
    }

    /// <summary>
    /// Asynchronously creates a new record mapping for a given project.
    /// </summary>
    /// <param name="projectId">The ID of the project in which to create the mapping</param>
    /// <param name="dto">The mapping request data transfer object containing mapping details</param>
    /// <returns>The created mapping response DTO with saved details</returns>
    public async Task<RecordMappingResponseDto> CreateRecordMapping(
        long projectId, 
        CreateRecordMappingRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        ValidationHelper.ValidateModel(dto);

        if (!dto.ClassId.HasValue && !dto.TagId.HasValue)
        {
            throw new InvalidRequestException("Both ClassID and TagID cannot be null. Please provide a value for at least one of these fields");
        }
        
        var mapping = new RecordMapping
        {
            RecordParams = dto.RecordParams.ToString(),
            ProjectId = projectId,
            ClassId = dto.ClassId,
            DataSourceId = dto.DataSourceId,
            TagId = dto.TagId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        };
        
        _context.RecordMappings.Add(mapping);
        await _context.SaveChangesAsync();
        
        // log mapping create event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            Operation = "create",
            EntityType = "record_mapping",
            EntityId = mapping.Id,
            ProjectId = mapping.ProjectId,
            Properties = "{}",
            DataSourceId = mapping.DataSourceId,
            CreatedBy = "", // TODO: add username when JWT are implemented
        });
        
        return new RecordMappingResponseDto
        {
            Id = mapping.Id,
            RecordParams = JsonNode.Parse(mapping.RecordParams) as JsonObject,
            ClassId = mapping.ClassId,
            ProjectId = mapping.ProjectId,
            DataSourceId = mapping.DataSourceId,
            TagId = mapping.TagId,
            CreatedBy = mapping.CreatedBy,
            CreatedAt = mapping.CreatedAt
        };
    }

    /// <summary>
    /// Updates an existing mapping by its ID
    /// </summary>
    /// <param name="projectId">The ID of the project to which the mapping belongs.</param>
    /// <param name="mappingId">The ID of the mapping to update</param>
    /// <param name="dto">The mapping request data transfer object containing updated details</param>
    /// <returns>The updated mapping response DTO with its details</returns>
    /// <exception cref="KeyNotFoundException">Returned if mapping not found</exception>
    public async Task<RecordMappingResponseDto> UpdateRecordMapping(
        long projectId,
        long mappingId,
        UpdateRecordMappingRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        ValidationHelper.ValidateModel(dto);
        
        var mapping = await _context.RecordMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.ArchivedAt is not null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} not found");
        }
        
        mapping.RecordParams = dto.RecordParams?.ToString() ?? mapping.RecordParams;
        mapping.ProjectId = projectId;
        mapping.ClassId = dto.ClassId ?? mapping.ClassId;
        mapping.DataSourceId = dto.DataSourceId ?? mapping.DataSourceId;
        mapping.TagId = dto.TagId ?? mapping.TagId;
        mapping.ModifiedBy = null; // TODO: handled in future by JWT.
        mapping.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        _context.RecordMappings.Update(mapping);
        await _context.SaveChangesAsync();
        
        // log mapping update event
        await _eventBusiness.CreateEvent( new CreateEventRequestDto
        {
            Operation = "update",
            EntityType = "record_mapping",
            EntityId = mapping.Id,
            ProjectId = mapping.ProjectId,
            Properties = "{}",
            DataSourceId = mapping.DataSourceId,
            CreatedBy = "", // TODO: add username when JWT are implemented
        });

        
        return new RecordMappingResponseDto
        {
            Id = mapping.Id,
            RecordParams = JsonNode.Parse(mapping.RecordParams) as JsonObject,
            ClassId = mapping.ClassId,
            ProjectId = mapping.ProjectId,
            TagId = mapping.TagId,
            DataSourceId = mapping.DataSourceId,
            CreatedBy = mapping.CreatedBy,
            CreatedAt = mapping.CreatedAt,
            ModifiedBy = mapping.ModifiedBy,
            ModifiedAt = mapping.ModifiedAt
        };
    }

    /// <summary>
    /// Deletes a specific mapping by its ID
    /// </summary>
    /// <param name="mappingId">The ID of the mapping to delete</param>
    /// <param name="projectId">The ID of the project to which the mapping belongs.</param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Returned if mapping not found</exception>
    public async Task<bool> DeleteRecordMapping(long projectId, long mappingId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var mapping = await _context.RecordMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId)
            throw new KeyNotFoundException($"Record Mapping with id {mappingId} not found");

        _context.RecordMappings.Remove(mapping);
        await _context.SaveChangesAsync();

        return true;
    }
    
    /// <summary>
    /// Archives a specific mapping by its ID
    /// </summary>
    /// <param name="mappingId">The ID of the mapping to archive</param>
    /// <param name="projectId">The ID of the project to which the mapping belongs.</param>
    /// <returns>Boolean true on successful archive.</returns>
    /// <exception cref="KeyNotFoundException">Returned if mapping not found</exception>
    public async Task<bool> ArchiveRecordMapping(long projectId, long mappingId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var mapping = await _context.RecordMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.ArchivedAt is not null)
            throw new KeyNotFoundException($"Record Mapping with id {mappingId} not found");

        mapping.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        _context.RecordMappings.Update(mapping);
        await _context.SaveChangesAsync();
        
        // Log Mapping soft delete event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "delete",
            EntityType = "record_mapping",
            EntityId = mapping.Id,
            DataSourceId = mapping.DataSourceId,
            Properties = "{}",
            CreatedBy = "" // TODO: add username when JWT are implemented
        });

        return true;
    }
    
    /// <summary>
    /// Unarchives a specific mapping by its ID
    /// </summary>
    /// <param name="mappingId">The ID of the mapping to unarchive</param>
    /// <param name="projectId">The ID of the project to which the mapping belongs.</param>
    /// <returns>Boolean true on successful unarchive.</returns>
    /// <exception cref="KeyNotFoundException">Returned if mapping not found</exception>
    public async Task<bool> UnarchiveRecordMapping(long projectId, long mappingId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var mapping = await _context.RecordMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.ArchivedAt is null)
            throw new KeyNotFoundException($"Record Mapping with id {mappingId} not found or is not archived.");

        mapping.ArchivedAt = null;
        _context.RecordMappings.Update(mapping);
        await _context.SaveChangesAsync();

        return true;
    }
    
    
}