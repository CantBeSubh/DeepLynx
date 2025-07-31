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
    /// <param name="hideArchived">Flag indicating whether to hide archived mappings from the result</param>
    /// <returns>A list of edge mappings based on the applied filters.</returns>
    public async Task<IEnumerable<EdgeMappingResponseDto>> GetAllEdgeMappings(
        long projectId,
        long? classId,
        long? relationshipId,
        bool hideArchived)
    {
        DoesProjectExist(projectId,  hideArchived);
        if (relationshipId.HasValue)
        {
            DoesRelationshipExist(relationshipId.Value, hideArchived);
        }
        
        var mappingQuery = _context.EdgeMappings
            .Where(e => e.ProjectId == projectId);
            
            // add filter for class or tag if specified                                  
            if (classId.HasValue)                                                        
            {                                                                            
                mappingQuery = mappingQuery.Where(m => m.OriginId == classId || m.DestinationId == classId);            
            }                                                                            
                                                                                 
            if (relationshipId.HasValue)                                                          
            {                                                                            
                mappingQuery = mappingQuery.Where(m => m.RelationshipId == relationshipId);                
            }
            
            if (hideArchived)
            {
                mappingQuery = mappingQuery.Where(m => m.ArchivedAt == null);
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
                DataSourceId = m.DataSourceId,
                ProjectId = m.ProjectId,
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
    public async Task<EdgeMappingResponseDto> GetEdgeMapping(
        long projectId, 
        long mappingId,
        bool hideArchived)
    {
        DoesProjectExist(projectId, hideArchived);
        var mapping = await _context.EdgeMappings
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

        return new EdgeMappingResponseDto
        {
            Id = mapping.Id,
            OriginParams = JsonNode.Parse(mapping.OriginParams) as JsonObject,
            DestinationParams = JsonNode.Parse(mapping.DestinationParams) as JsonObject,
            RelationshipId = mapping.RelationshipId,
            OriginId = mapping.OriginId,
            DestinationId = mapping.DestinationId,
            DataSourceId = mapping.DataSourceId,
            ProjectId = mapping.ProjectId,
            CreatedBy = mapping.CreatedBy,
            CreatedAt = mapping.CreatedAt,
            ModifiedBy = mapping.ModifiedBy,
            ModifiedAt = mapping.ModifiedAt,
            ArchivedAt = mapping.ArchivedAt,
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
        CreateEdgeMappingRequestDto dto)
    {
        DoesProjectExist(projectId);
        var mapping = new EdgeMapping
        {
            ProjectId = projectId,
            OriginParams = dto.OriginParams.ToString(),
            DestinationParams = dto.DestinationParams.ToString(),
            RelationshipId = dto.RelationshipId,
            DataSourceId = dto.DataSourceId,
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
            DataSourceId = mapping.DataSourceId,
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
        UpdateEdgeMappingRequestDto dto)
    {
        DoesProjectExist(projectId);
       
        var mapping = await _context.EdgeMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.ArchivedAt is not null)
        {
            throw new KeyNotFoundException($"Mapping with id {mappingId} not found");
        }
        
        mapping.OriginParams = dto.OriginParams?.ToString() ?? mapping.OriginParams.ToString();
        mapping.DestinationParams = dto.DestinationParams?.ToString() ?? mapping.DestinationParams;
        mapping.RelationshipId = dto.RelationshipId ?? mapping.RelationshipId;
        mapping.OriginId = dto.OriginId ?? mapping.OriginId;
        mapping.DestinationId = dto.DestinationId ?? mapping.DestinationId;
        mapping.DataSourceId = dto.DataSourceId ?? mapping.DataSourceId;
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
            DataSourceId = mapping.DataSourceId,
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
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Returned if mapping not found</exception>
    public async Task<bool> DeleteEdgeMapping(long projectId, long mappingId)
    {
        DoesProjectExist(projectId);
        var mapping = await _context.EdgeMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId)
            throw new KeyNotFoundException($"Edge Mapping with id {mappingId} not found");

        _context.EdgeMappings.Remove(mapping);
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
    public async Task<bool> ArchiveEdgeMapping(long projectId, long mappingId)
    {
        DoesProjectExist(projectId);
        var mapping = await _context.EdgeMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.ArchivedAt is not null)
            throw new KeyNotFoundException($"Edge Mapping with id {mappingId} not found");

        mapping.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        _context.EdgeMappings.Update(mapping);
        await _context.SaveChangesAsync();

        return true;
    }
    
    /// <summary>
    /// Unarchives a specific mapping by its ID
    /// </summary>
    /// <param name="mappingId">The ID of the mapping to unarchive</param>
    /// <param name="projectId">The ID of the project to which the mapping belongs.</param>
    /// <returns>Boolean true on successful unarchive.</returns>
    /// <exception cref="KeyNotFoundException">Returned if mapping not found</exception>
    public async Task<bool> UnarchiveEdgeMapping(long projectId, long mappingId)
    {
        DoesProjectExist(projectId);
        var mapping = await _context.EdgeMappings.FindAsync(mappingId);

        if (mapping == null || mapping.ProjectId != projectId || mapping.ArchivedAt is null)
            throw new KeyNotFoundException($"Edge Mapping with id {mappingId} not found or is not archived.");

        mapping.ArchivedAt = null;
        _context.EdgeMappings.Update(mapping);
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
    
    /// <summary>
    /// Determine if relationship exists
    /// </summary>
    /// <param name="relationshipId">The ID of the relationship we are searching for</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
    /// <returns>Throws error if relationship does not exist</returns>
    private void DoesRelationshipExist(long relationshipId, bool hideArchived = true)
    {
        var relationship = hideArchived ? _context.Relationships.Any(p => p.Id == relationshipId && p.ArchivedAt == null)
            :  _context.Relationships.Any(p => p.Id == relationshipId);
        if (!relationship)
        {
            throw new KeyNotFoundException($"Relationship with id {relationshipId} not found");
        }
    }
}