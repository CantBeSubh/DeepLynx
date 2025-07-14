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
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result</param>
    /// <returns>A list of edges based on the applied filters.</returns>
    public async Task<IEnumerable<EdgeResponseDto>> GetAllEdges(
        long projectId, 
        long? dataSourceId,
        bool hideArchived)
    {
        DoesProjectExist(projectId);
        if (dataSourceId.HasValue)
        { 
            DoesDataSourceExist(dataSourceId.Value);
        }
       
        // base query object to get all edges for the project
        var edgeQuery = _context.Edges
            .Where(e => e.ProjectId == projectId);
    
        if (dataSourceId.HasValue)
        {
            edgeQuery = edgeQuery.Where(e => e.DataSourceId == dataSourceId);
        }
        
        if (hideArchived)
        {
            edgeQuery = edgeQuery.Where(e => e.ArchivedAt == null);
        }
        
        var edges = await edgeQuery.ToListAsync();

        return edges.Select(e => new EdgeResponseDto()
            {
                Id = e.Id,
                OriginId = e.OriginId,
                DestinationId = e.DestinationId,
                RelationshipId = e.RelationshipId,
                DataSourceId = e.DataSourceId,
                ProjectId = e.ProjectId,
                CreatedAt = e.CreatedAt,
                CreatedBy = e.CreatedBy,
                ModifiedAt = e.ModifiedAt,
                ModifiedBy = e.ModifiedBy,
                ArchivedAt = e.ArchivedAt,
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
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result</param>
    /// <returns>The edge associated with the given id or origin/destination combo</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or is archived</exception>
    public async Task<EdgeResponseDto> GetEdge(long? edgeId, long? originId, long? destinationId, bool hideArchived)
    {
        var edge = await FindEdge(edgeId, originId, destinationId);
        
        if (hideArchived && edge.ArchivedAt != null)
        {
            throw new KeyNotFoundException($"Edge with id {edgeId} is archived");
        }

        return new EdgeResponseDto
        {
            Id = edge.Id,
            OriginId = edge.OriginId,
            DestinationId = edge.DestinationId,
            RelationshipId = edge.RelationshipId,
            DataSourceId = edge.DataSourceId,
            ProjectId = edge.ProjectId,
            CreatedAt = edge.CreatedAt,
            CreatedBy = edge.CreatedBy,
            ModifiedAt = edge.ModifiedAt,
            ModifiedBy = edge.ModifiedBy,
            ArchivedAt = edge.ArchivedAt,
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
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        
        var edge = new Edge
        {
            OriginId = dto.OriginId,
            DestinationId = dto.DestinationId,
            ProjectId = projectId,
            DataSourceId = dataSourceId,
            RelationshipId = dto.RelationshipId,
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
            RelationshipId = edge.RelationshipId,
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
        DoesProjectExist(projectId);
        // find edge and perform error handling if not found
        var edge = await FindEdge(edgeId, originId, destinationId);
        if (edge == null || edge.ProjectId != projectId || edge.ArchivedAt is not null)
        {
            throw new KeyNotFoundException("Edge may have been moved or deleted.");
        }
        
        edge.OriginId = dto.OriginId;
        edge.DestinationId = dto.DestinationId;
        edge.RelationshipId = dto.RelationshipId;
        edge.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        edge.ModifiedBy = null;  // TODO: Implement user ID here when JWT tokens are ready
        
        _context.Edges.Update(edge);
        await _context.SaveChangesAsync();
        
        return new EdgeResponseDto
        {
            Id = edge.Id,
            OriginId = edge.OriginId,
            DestinationId = edge.DestinationId,
            RelationshipId = edge.RelationshipId,
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
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<long> DeleteEdge(
        long projectId, 
        long? edgeId,
        long? originId, 
        long? destinationId)
    {
        DoesProjectExist(projectId);
        // find edge and perform error handling if not found
        var edge = await FindEdge(edgeId, originId, destinationId);
        if (edge == null || edge.ProjectId != projectId || edge.ArchivedAt is not null) 
            throw new KeyNotFoundException("Edge may have been moved or deleted.");

        _context.Edges.Remove(edge);
        await _context.SaveChangesAsync();
        return edge.Id;
    }
    
    /// <summary>
    /// Archives a specific edge by its ID or origin/destination.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the edge belongs.</param>
    /// <param name="edgeId">The ID of the edge to archive</param>
    /// <param name="originId">The origin ID of the edge to archive if edgeID is not present.</param>
    /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<long> ArchiveEdge(
        long projectId, 
        long? edgeId,
        long? originId, 
        long? destinationId)
    {
        DoesProjectExist(projectId);
        // find edge and perform error handling if not found
        var edge = await FindEdge(edgeId, originId, destinationId);
        if (edge == null || edge.ProjectId != projectId || edge.ArchivedAt is not null) 
            throw new KeyNotFoundException("Edge may have been moved, archived or deleted.");

        edge.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        _context.Edges.Update(edge);
        await _context.SaveChangesAsync();
        return edge.Id;
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
    /// Determine if project exists
    /// </summary>
    /// <param name="projectId">The ID of the project we are searching for</param>
    /// <returns>Throws error if project does not exist</returns>
    private void DoesProjectExist(long projectId)
    {
        var project = _context.Projects.Any(p => p.Id == projectId && p.ArchivedAt == null);
        if (!project)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }
    }
    
    /// <summary>
    /// Determine if datasource exists
    /// </summary>
    /// <param name="datasourceId">The ID of the datasource we are searching for</param>
    /// <returns>Throws error if datasource does not exist</returns>
    private void DoesDataSourceExist(long datasourceId)
    {
        var datasource = _context.DataSources.Any(p => p.Id == datasourceId && p.ArchivedAt == null);
        if (!datasource)
        {
            throw new KeyNotFoundException($"Datasource with id {datasourceId} not found");
        }
    }
}