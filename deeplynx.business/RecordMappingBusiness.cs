using System.Linq.Expressions;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using deeplynx.helpers.exceptions;
using Microsoft.EntityFrameworkCore.Storage;

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
    /// Bulk Soft Delete record mappings by a specific upstream domain. Used to avoid repeating functions.
    /// </summary>
    /// <param name="predicate">an anonymous function that allows the context to be filtered appropriately</param>
    /// <param name="transaction">(Optional) a transaction passed in from the parent to ensure ACID compliance</param>
    /// <returns>Boolean true on successful deletion</returns>
    public async Task<bool> BulkSoftDeleteRecordMappings(Expression<Func<RecordMapping, bool>> predicate)
    {
        try
        {
            // search for record mappings matching the passed-in predicate (filter) to be updated
            var mContext = _context.RecordMappings
                .Where(m => m.DeletedAt == null)
                .Where(predicate);
            
            var recordMappings = await mContext.ToListAsync();
    
            if (recordMappings.Count == 0)
            {
                // return early if there are no record mappings to delete
                return true;
            }
            
            var mappingIds = recordMappings.Select(r => r.Id);
    
            var updated = await mContext.ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.DeletedAt, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)));
    
            // if we found mappings to update, but weren't successful in updating, throw an error
            if (updated == 0)
            {
                throw new DependencyDeletionException("An error occurred when deleting record mappings");
            }
            
            // save changes
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting record mappings: {exc}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
}