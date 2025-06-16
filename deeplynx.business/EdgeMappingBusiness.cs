using System.Linq.Expressions;
using deeplynx.interfaces;                        
using deeplynx.datalayer.Models;                  
using deeplynx.models;                            
using Microsoft.EntityFrameworkCore;              
using System.Text.Json.Nodes;
using deeplynx.helpers.exceptions;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.business;                      

public class EdgeMappingBusiness : IEdgeMappingBusiness
{
    private readonly DeeplynxContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeMappingBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the edge mapping operations.</param>
    public EdgeMappingBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all edge mappings for a specific project and (optionally) class or relationship
    /// </summary>
    /// <param name="projectId">The ID of the project whose mappings are to be retrieved</param>
    /// <param name="classId">(Optional) the ID of the origin or destination class by which to filter mappings</param>
    /// <param name="relationshipId">(Optional) the ID of the relationship by which to filter mappings</param>
    /// <returns>A list of edge mappings based on the applied filters.</returns>
    public async Task<IEnumerable<EdgeMappingResponseDto>> GetAllEdgeMappings(
        long projectId,
        long? classId,
        long? relationshipId)
    {
        var mappingQuery = _context.EdgeMappings
            .Where(e => e.ProjectId == projectId && e.DeletedAt == null);
            
            // add filter for class or tag if specified                                  
            if (classId.HasValue)                                                        
            {                                                                            
                mappingQuery = mappingQuery.Where(m => m.OriginId == classId || m.DestinationId == classId);            
            }                                                                            
                                                                                 
            if (relationshipId.HasValue)                                                          
            {                                                                            
                mappingQuery = mappingQuery.Where(m => m.RelationshipId == relationshipId);                
            }
            
            var mappings = await mappingQuery.ToListAsync();
                
            return mappings
            .Select(m => new EdgeMappingResponseDto()
            {
                Id = m.Id,
                OriginParams = JsonNode.Parse(m.OriginParams) as JsonObject,
                DestinationParams = JsonNode.Parse(m.DestinationParams) as JsonObject,
                RelationshipId = m.RelationshipId,
                OriginId = m.OriginId,
                DestinationId = m.DestinationId,
                ProjectId = m.ProjectId,
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
    public async Task<EdgeMappingResponseDto> GetEdgeMapping(
        long projectId, 
        long mappingId)
    {
        var mapping = await _context.EdgeMappings
            .Where(m => m.Id == mappingId && m.ProjectId == projectId && m.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (mapping == null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} not found");
        }

        return new EdgeMappingResponseDto
        {
            Id = mapping.Id,
            OriginParams = JsonNode.Parse(mapping.OriginParams) as JsonObject,
            DestinationParams = JsonNode.Parse(mapping.DestinationParams) as JsonObject,
            RelationshipId = mapping.RelationshipId,
            OriginId = mapping.OriginId,
            DestinationId = mapping.DestinationId,
            ProjectId = mapping.ProjectId,
            CreatedBy = mapping.CreatedBy,
            CreatedAt = mapping.CreatedAt,
            ModifiedBy = mapping.ModifiedBy,
            ModifiedAt = mapping.ModifiedAt
        };
    }

    /// <summary>
    /// Asynchronously creates a new edge mapping for a given project.
    /// </summary>
    /// <param name="projectId">The ID of the project in which to create the mapping</param>
    /// <param name="dto">The mapping request data transfer object containing mapping details</param>
    /// <returns>The created mapping response DTO with saved details</returns>
    public async Task<EdgeMappingResponseDto> CreateEdgeMapping(
        long projectId,
        EdgeMappingRequestDto dto)
    {
        var mapping = new EdgeMapping
        {
            ProjectId = projectId,
            OriginParams = dto.OriginParams.ToString(),
            DestinationParams = dto.DestinationParams.ToString(),
            RelationshipId = dto.RelationshipId,
            OriginId = dto.OriginId,
            DestinationId = dto.DestinationId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        };
        
        _context.EdgeMappings.Add(mapping);
        await _context.SaveChangesAsync();
        
        return new EdgeMappingResponseDto
        {
            Id = mapping.Id,
            OriginParams = JsonNode.Parse(mapping.OriginParams) as JsonObject,
            DestinationParams = JsonNode.Parse(mapping.DestinationParams) as JsonObject,
            RelationshipId = mapping.RelationshipId,
            OriginId = mapping.OriginId,
            DestinationId = mapping.DestinationId,
            ProjectId = mapping.ProjectId,
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
    public async Task<EdgeMappingResponseDto> UpdateEdgeMapping(
        long projectId,
        long mappingId,
        EdgeMappingRequestDto dto)
    {
        var mapping = await _context.EdgeMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} not found");
        }
        
        mapping.OriginParams = dto.OriginParams.ToString();
        mapping.DestinationParams = dto.DestinationParams.ToString();
        mapping.RelationshipId = dto.RelationshipId;
        mapping.OriginId = dto.OriginId;
        mapping.DestinationId = dto.DestinationId;
        mapping.ProjectId = projectId;
        mapping.ModifiedBy = null; // TODO: handled in future by JWT.
        mapping.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        _context.EdgeMappings.Update(mapping);
        await _context.SaveChangesAsync();
        
        return new EdgeMappingResponseDto
        {
            Id = mapping.Id,
            OriginParams = JsonNode.Parse(mapping.OriginParams) as JsonObject,
            DestinationParams = JsonNode.Parse(mapping.DestinationParams) as JsonObject,
            RelationshipId = mapping.RelationshipId,
            OriginId = mapping.OriginId,
            DestinationId = mapping.DestinationId,
            ProjectId = mapping.ProjectId,
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
    public async Task<bool> DeleteEdgeMapping(
        long projectId, 
        long mappingId, 
        bool force=false)
    {
        var mapping = await _context.EdgeMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} not found");
        }

        if (force)
        {
            _context.EdgeMappings.Remove(mapping);
        }
        else
        {
            // soft delete
            mapping.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.EdgeMappings.Update(mapping);
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Bulk Soft Delete edge mappings by a specific upstream domain. Used to avoid repeating functions.
    /// </summary>
    /// <param name="domainType">The type of domain which is calling this function</param>
    /// <param name="domainId">The ID of the upstream domain calling this function</param>
    /// <returns>Boolean true on successful deletion</returns>
    public async Task<bool> BulkSoftDeleteEdgeMappings(Expression<Func<EdgeMapping, bool>> predicate)
    {
        try
        {
            // search for records matching the passed-in predicate (filter) to be updated
            var emContext = _context.EdgeMappings
                .Where(d => d.DeletedAt == null)
                .Where(predicate);

            var edgeMappings = await emContext.ToListAsync();
            
            if (edgeMappings.Count == 0)
            {
                // return early if no edge mappings are to be deleted
                return true;
            }

            // bulk update the results of the query to set the deleted_at date
            var updated = await emContext.ExecuteUpdateAsync(setters => setters
                .SetProperty(ds => ds.DeletedAt, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)));

            // if we found edge mappings to update, but weren't successful in updating, throw an error
            if (updated == 0)
            {
                throw new DependencyDeletionException("Edge mappings found but were not deleted");
            }

            // save changes and commit transaction to close it
            await _context.SaveChangesAsync();
            return true;
                
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting edge mappings: {exc}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
}