using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

namespace deeplynx.business;

public class RecordMappingBusiness : IRecordMappingBusiness
{
    private readonly DeeplynxContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordMappingBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the record mapping operations.</param>
    public RecordMappingBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all record mappings for a specific project and (optionally) class or tag.
    /// </summary>
    /// <param name="projectId">The ID of the project whose mappings are to be retrieved</param>
    /// <param name="classId">(Optional) the ID of the class by which to filter mappings</param>
    /// <param name="tagId">(Optional) the ID of the tag by which to filter mappings</param>
    /// <returns>A list of record mappings based on the applied filters.</returns>
    public async Task<IEnumerable<RecordMappingResponseDto>> GetAllRecordMappings(
        long projectId,
        long? classId,
        long? tagId)
    {
        var mappingQuery = _context.RecordMappings
            .Where(m => m.ProjectId == projectId && m.DeletedAt == null);

        // add filter for class or tag if specified
        if (classId.HasValue)
        {
            mappingQuery = mappingQuery.Where(m => m.ClassId == classId);
        }

        if (tagId.HasValue)
        {
            mappingQuery = mappingQuery.Where(m => m.TagId == tagId);
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
                CreatedBy = m.CreatedBy,
                CreatedAt = m.CreatedAt,
                ModifiedBy = m.ModifiedBy,
                ModifiedAt = m.ModifiedAt
            })
            .ToList();
    }

    /// <summary>
    /// Retrieves a specific mapping by its id
    /// </summary>
    /// <param name="mappingId">The id whereby to fetch the mapping</param>
    /// <param name="projectId">The project ID for the project to which the mapping belongs</param>
    /// <returns>The mapping associated with the given ID</returns>
    /// <exception cref="KeyNotFoundException">Returned if mapping not found</exception>
    public async Task<RecordMappingResponseDto> GetRecordMapping(
        long projectId,
        long mappingId
        )
    {
        var mapping = await _context.RecordMappings
            .Where(m => m.Id == mappingId && m.ProjectId == projectId && m.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (mapping == null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} not found");
        }

        return new RecordMappingResponseDto
        {
            Id = mapping.Id,
            RecordParams = JsonNode.Parse(mapping.RecordParams) as JsonObject,
            ClassId = mapping.ClassId,
            ProjectId = mapping.ProjectId,
            TagId = mapping.TagId,
            CreatedBy = mapping.CreatedBy,
            CreatedAt = mapping.CreatedAt,
            ModifiedBy = mapping.ModifiedBy,
            ModifiedAt = mapping.ModifiedAt
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
        RecordMappingRequestDto dto)
    {
        var mapping = new RecordMapping
        {
            RecordParams = dto.RecordParams.ToString(),
            ProjectId = projectId,
            ClassId = dto.ClassId,
            TagId = dto.TagId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        };
        
        _context.RecordMappings.Add(mapping);
        await _context.SaveChangesAsync();
        
        return new RecordMappingResponseDto
        {
            Id = mapping.Id,
            RecordParams = JsonNode.Parse(mapping.RecordParams) as JsonObject,
            ClassId = mapping.ClassId,
            ProjectId = mapping.ProjectId,
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
        RecordMappingRequestDto dto)
    {
        var mapping = await _context.RecordMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} not found");
        }
        
        mapping.RecordParams = dto.RecordParams.ToString();
        mapping.ProjectId = projectId;
        mapping.ClassId = dto.ClassId;
        mapping.TagId = dto.TagId;
        mapping.ModifiedBy = null; // TODO: handled in future by JWT.
        mapping.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        _context.RecordMappings.Update(mapping);
        await _context.SaveChangesAsync();
        
        return new RecordMappingResponseDto
        {
            Id = mapping.Id,
            RecordParams = JsonNode.Parse(mapping.RecordParams) as JsonObject,
            ClassId = mapping.ClassId,
            ProjectId = mapping.ProjectId,
            TagId = mapping.TagId,
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
    /// <param name="force">Indicates whether to force delete the mapping if true.</param>
    /// <exception cref="KeyNotFoundException">Returned if mapping not found</exception>
    public async Task<bool> DeleteRecordMapping(
        long projectId, 
        long mappingId, 
        bool force=false)
    {
        var mapping = await _context.RecordMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} not found");
        }

        if (force)
        {
            _context.RecordMappings.Remove(mapping);
        }
        else
        {
            // soft delete
            mapping.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.RecordMappings.Update(mapping);
        }
        
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Called primarily by project's delete. Soft delete all record mappings in a project by project id.
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<bool> SoftDeleteAllRecordMappingsByProjectIdAsync(long projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null)
            throw new KeyNotFoundException("Project not found.");

        try
        {
            var recordMappings = await _context.RecordMappings
                .Where(t => t.ProjectId == projectId && t.DeletedAt == null).ToListAsync();
            foreach (var recordMapping in recordMappings)
            {
                recordMapping.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while deleting project record mappings: {exception}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }

}