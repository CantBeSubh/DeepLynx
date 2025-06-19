using System.Linq.Expressions;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using deeplynx.helpers.exceptions;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.business;

public class EdgeBusiness : IEdgeBusiness
{
    private readonly DeeplynxContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the edge operations.</param>
    public EdgeBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all edges for a specific project and (optionally) datasource
    /// </summary>
    /// <param name="projectId">The ID of the project whose edges are to be retrieved</param>
    /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter edges</param>
    /// <returns>A list of edges based on the applied filters.</returns>
    public async Task<IEnumerable<EdgeResponseDto>> GetAllEdges(
        long projectId, 
        long? dataSourceId)
    {
        // base query object to get all edges for the project
        var edgeQuery = _context.Edges
            .Where(e => e.ProjectId == projectId && e.ArchivedAt == null);
    
        // add filter for datasource if specified
        if (dataSourceId.HasValue)
        {
            edgeQuery = edgeQuery.Where(e => e.DataSourceId == dataSourceId);
        }
        
        var edges = await edgeQuery.ToListAsync();

        // execute query and return results
        return edges.Select(e => new EdgeResponseDto()
            {
                Id = e.Id,
                OriginId = e.OriginId,
                DestinationId = e.DestinationId,
                // return empty object for properties if null
                Properties = JsonNode.Parse(e.Properties ?? "{}") as JsonObject, 
                RelationshipId = e.RelationshipId,
                RelationshipName = e.RelationshipName,
                DataSourceId = e.DataSourceId,
                ProjectId = e.ProjectId,
                CreatedAt = e.CreatedAt,
                CreatedBy = e.CreatedBy,
                ModifiedAt = e.ModifiedAt,
                ModifiedBy = e.ModifiedBy
            })
            .ToList();
    }

    /// <summary>
    /// Retrieves a specific edge by its origin and destination IDs
    /// OR Retrieves an edge by its id
    /// </summary>
    /// <param name="edgeId">The id whereby to fetch the edge</param>
    /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
    /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
    /// <returns>The edge associated with the given id or origin/destination combo</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<EdgeResponseDto> GetEdge(long? edgeId, long? originId, long? destinationId)
    {
        var edge = await FindEdge(edgeId, originId, destinationId);

        return new EdgeResponseDto
        {
            Id = edge.Id,
            OriginId = edge.OriginId,
            DestinationId = edge.DestinationId,
            // return empty object for properties if null
            Properties = JsonNode.Parse(edge.Properties ?? "{}") as JsonObject,
            RelationshipId = edge.RelationshipId,
            RelationshipName = edge.RelationshipName,
            DataSourceId = edge.DataSourceId,
            ProjectId = edge.ProjectId,
            CreatedAt = edge.CreatedAt,
            CreatedBy = edge.CreatedBy,
            ModifiedAt = edge.ModifiedAt,
            ModifiedBy = edge.ModifiedBy
        };
    }

    /// <summary>
    /// Asynchronously creates a new edge for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
    /// <param name="dto">The edge request data transfer object containing edge details</param>
    /// <returns>The created edge response DTO with saved details.</returns>
    public async Task<EdgeResponseDto> CreateEdge(
        long projectId, 
        long dataSourceId, 
        EdgeRequestDto dto)
    {
        var edge = new Edge
        {
            OriginId = dto.OriginId,
            DestinationId = dto.DestinationId,
            ProjectId = projectId,
            DataSourceId = dataSourceId,
            Properties = dto.Properties?.ToString(),
            RelationshipId = dto.RelationshipId,
            RelationshipName = dto.RelationshipName,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        };
        
        _context.Edges.Add(edge);
        await _context.SaveChangesAsync();
        
        return new EdgeResponseDto
        {
            Id = edge.Id,
            OriginId = edge.OriginId,
            DestinationId = edge.DestinationId,
            // return empty object for properties if null
            Properties = JsonNode.Parse(edge.Properties ?? "{}") as JsonObject,
            RelationshipId = edge.RelationshipId,
            RelationshipName = edge.RelationshipName,
            DataSourceId = edge.DataSourceId,
            ProjectId = edge.ProjectId,
            CreatedAt = edge.CreatedAt,
            CreatedBy = edge.CreatedBy
        };
    }

    /// <summary>
    /// Updates an existing edge by its ID or origin/destination.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the edge belongs.</param>
    /// <param name="dto">The edge request data transfer object containing updated edge details.</param>
    /// <param name="edgeId">The ID of the edge to update</param>
    /// <param name="originId">The origin ID of the edge to update if edgeID is not present.</param>
    /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
    /// <returns>The updated edge response DTO with its details</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<EdgeResponseDto> UpdateEdge(
        long projectId,
        EdgeRequestDto dto,
        long? edgeId,
        long? originId, 
        long? destinationId)
    {
        // find edge and perform error handling if not found
        var edge = await FindEdge(edgeId, originId, destinationId);
        if (edge == null || edge.ProjectId != projectId || edge.ArchivedAt is not null)
        {
            throw new KeyNotFoundException("Edge may have been moved or deleted.");
        }
        
        edge.OriginId = dto.OriginId;
        edge.DestinationId = dto.DestinationId;
        edge.Properties = dto.Properties?.ToString();
        edge.RelationshipId = dto.RelationshipId;
        edge.RelationshipName = dto.RelationshipName;
        edge.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        edge.ModifiedBy = null;  // TODO: Implement user ID here when JWT tokens are ready
        
        _context.Edges.Update(edge);
        await _context.SaveChangesAsync();
        
        return new EdgeResponseDto
        {
            Id = edge.Id,
            OriginId = edge.OriginId,
            DestinationId = edge.DestinationId,
            // return empty object for properties if null
            Properties = JsonNode.Parse(edge.Properties ?? "{}") as JsonObject,
            RelationshipId = edge.RelationshipId,
            RelationshipName = edge.RelationshipName,
            DataSourceId = edge.DataSourceId,
            ProjectId = edge.ProjectId,
            CreatedAt = edge.CreatedAt,
            CreatedBy = edge.CreatedBy,
            ModifiedAt = edge.ModifiedAt,
            ModifiedBy = edge.ModifiedBy
        };
    }

    /// <summary>
    /// Deletes a specific edge by its ID or origin/destination.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the edge belongs.</param>
    /// <param name="edgeId">The ID of the edge to delete</param>
    /// <param name="originId">The origin ID of the edge to delete if edgeID is not present.</param>
    /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
    /// <param name="force">Indicates whether to force delete the edge if true.</param>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<bool> DeleteEdge(
        long projectId, 
        long? edgeId,
        long? originId, 
        long? destinationId,
        bool force=false)
    {
        // find edge and perform error handling if not found
        var edge = await FindEdge(edgeId, originId, destinationId);
        if (edge == null || edge.ProjectId != projectId || edge.ArchivedAt is not null)
        {
            throw new KeyNotFoundException("Edge may have been moved or deleted.");
        }

        if (force)
        {
            _context.Edges.Remove(edge);
        }
        else
        {
            // soft delete
            edge.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.Edges.Update(edge);
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Private method to facilitate boilerplate code for finding edges by ID or origindestination
    /// </summary>
    /// <param name="edgeId">The id whereby to fetch the edge</param>
    /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
    /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
    /// <returns>The edge associated with the given id or origin/destination combo</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    private async Task<Edge> FindEdge(long? edgeId, long? originId, long? destinationId)
    {
        if (edgeId == null && (originId == null || destinationId == null))
        {
            throw new KeyNotFoundException("Please supply either an edgeID or an originID and destinationID");
        }
        
        Edge edge = null;

        // search for edge either by id or origin + destination
        if (edgeId != null)
        {
            edge = await _context.Edges
                .Where(e => e.Id == edgeId && e.ArchivedAt == null)
                .FirstOrDefaultAsync();
        }
        else
        {
            edge = await _context.Edges
                .Where(e => e.OriginId == originId && e.DestinationId == destinationId)
                .Where(e => e.ArchivedAt == null)
                .FirstOrDefaultAsync();
        }

        // throw an error if edge not found
        if (edge == null)
        {
            if (edgeId != null)
            {
                throw new KeyNotFoundException($"Edge with id {edgeId} not found");
            }
            else
            {
                throw new KeyNotFoundException($"Edge with origin {originId} and destination {destinationId} not found");
            }
        }

        return edge;  
    }

    /// <summary>
    /// Bulk Soft Delete edges by a specific upstream domain. Used to avoid repeating functions.
    /// </summary>
    /// <param name="predicate">an anonymous function that allows the context to be filtered appropriately</param>
    /// <returns>Boolean true on successful deletion</returns>
    public async Task<bool> BulkSoftDeleteEdges(Expression<Func<Edge, bool>> predicate)
    {
        try
        {
            // search for records matching the passed-in predicate (filter) to be updated
            var eContext = _context.Edges
                .Where(d => d.ArchivedAt == null)
                .Where(predicate);

            var edges = await eContext.ToListAsync();
            
            if (edges.Count == 0)
            {
                // return early if no edges are to be deleted
                return true;
            }

            // bulk update the results of the query to set the archived_at date
            var updated = await eContext.ExecuteUpdateAsync(setters => setters
                .SetProperty(ds => ds.ArchivedAt, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)));

            // if we found edges to update, but weren't successful in updating, throw an error
            if (updated == 0)
            {
                throw new DependencyDeletionException("Edges found but were not deleted");
            }

            // save changes and commit transaction to close it
            await _context.SaveChangesAsync();
            return true;
                
        }
        catch (Exception exc)
        {
            
            var message = $"An error occurred while deleting edges: {exc}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
    
}