using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class HistoricalEdgeBusiness : IHistoricalEdgeBusiness
{
    private readonly DeeplynxContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoricalEdgeBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the edge operations.</param>
    public HistoricalEdgeBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all Historical Edges for a specific project and datasource
    /// </summary>
    /// <param name="projectId">The ID of the project whose edges are to be retrieved</param>
    /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter edges</param>
    /// <param name="pointInTime">(Optional) Find the most current edges that existed before this point in time</param>
    /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived edges from the result.</param>
    /// <param name="current">(Optional) Find only the most current edges. Overrides point in time.</param>
    /// <returns>An array of edges</returns>
    /// TODO: create an endpoint for this
    public async Task<IEnumerable<HistoricalEdgeResponseDto>> GetAllHistoricalEdges(
        long projectId,
        long? dataSourceId = null,
        DateTime? pointInTime = null,
        bool hideArchived = true,
        bool current = true)
    {
        var edgeQuery = _context.HistoricalEdges
            .Where(e => e.ProjectId == projectId);

        if (dataSourceId.HasValue)
        {
            edgeQuery = edgeQuery.Where(e => e.DataSourceId == dataSourceId);
        }

        if (current)
        {
            edgeQuery = edgeQuery.Where(e => e.Current);
        }

        if (hideArchived)
        {
            edgeQuery = edgeQuery.Where(e => e.ArchivedAt == null);
        }
        
        // specification for "current" should override any supplied pointInTime
        if (pointInTime.HasValue && !current)
        {
            // compare timestamp to the most recent update
            edgeQuery = edgeQuery
                .Where(e => e.LastUpdatedAt <= pointInTime)
                .OrderByDescending(e => e.LastUpdatedAt);
        }

        return await edgeQuery
            .Select(e => new HistoricalEdgeResponseDto()
            {
                Id = e.EdgeId,
                OriginId = e.OriginId,
                DestinationId = e.DestinationId,
                RelationshipId = e.RelationshipId,
                RelationshipName = e.RelationshipName,
                MappingId = e.MappingId,
                DataSourceId = e.DataSourceId,
                ProjectId = e.ProjectId,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                ModifiedBy = e.ModifiedBy,
                ModifiedAt = e.ModifiedAt,
                ArchivedAt = e.ArchivedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Show the historical updates of a specific edge
    /// </summary>
    /// <param name="edgeId">The ID of the edge to list history for</param>
    /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
    /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
    /// <returns>An array of edge instances for the given edge</returns>
    /// TODO: create an endpoint for this
    public async Task<IEnumerable<HistoricalEdgeResponseDto>> GetHistoryForEdge(
        long? edgeId,
        long? originId, 
        long? destinationId)
    {
        var foundEdge = await FindEdge(edgeId, originId, destinationId);
        var foundEdgeId = foundEdge.EdgeId;
        
        return await _context.HistoricalEdges
            .Where(e => e.EdgeId == foundEdgeId)
            .OrderByDescending(e => e.LastUpdatedAt)
            .Select(e => new HistoricalEdgeResponseDto()
            {
                Id = e.EdgeId,
                OriginId = e.OriginId,
                DestinationId = e.DestinationId,
                RelationshipId = e.RelationshipId,
                RelationshipName = e.RelationshipName,
                MappingId = e.MappingId,
                DataSourceId = e.DataSourceId,
                ProjectId = e.ProjectId,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                ModifiedBy = e.ModifiedBy,
                ModifiedAt = e.ModifiedAt,
                ArchivedAt = e.ArchivedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Find an edge at a given point in time
    /// </summary>
    /// <param name="edgeId">The ID of the edge to retrieve</param>
    /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
    /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
    /// <param name="pointInTime">(Optional) Find the most current edge that existed before this point in time</param>
    /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived edges from the result.</param>
    /// <param name="current">(Optional) Find only the most current edge. Overrides point in time.</param>
    /// <returns>An edge that matches the applied filters.</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found</exception>
    /// /// TODO: create an endpoint for this
    public async Task<HistoricalEdgeResponseDto> GetHistoricalEdge(
        long? edgeId,
        long? originId, 
        long? destinationId,
        DateTime? pointInTime,
        bool hideArchived = true,
        bool current = true)
    {
        var foundEdge = await FindEdge(edgeId, originId, destinationId);
        var foundEdgeId = foundEdge.EdgeId;
        
        var edgeQuery = _context.HistoricalEdges
            .Where(e => e.EdgeId == foundEdgeId);

        if (current)
        {
            edgeQuery = edgeQuery.Where(e => e.Current);
        }

        if (pointInTime.HasValue && !current)
        {
            edgeQuery = edgeQuery
                .Where(e => e.LastUpdatedAt <= pointInTime)
                .OrderByDescending(e => e.LastUpdatedAt);
        }

        if (hideArchived)
        {
            edgeQuery = edgeQuery.Where(e => e.ArchivedAt == null);
        }
        
        var edge = await edgeQuery.FirstOrDefaultAsync();

        if (edge == null)
        {
            throw new KeyNotFoundException($"Edge with id {foundEdgeId} not found at point in time {pointInTime}.");
        }

        return new HistoricalEdgeResponseDto()
        {
            Id = edge.EdgeId,
            OriginId = edge.OriginId,
            DestinationId = edge.DestinationId,
            RelationshipId = edge.RelationshipId,
            RelationshipName = edge.RelationshipName,
            MappingId = edge.MappingId,
            DataSourceId = edge.DataSourceId,
            ProjectId = edge.ProjectId,
            CreatedBy = edge.CreatedBy,
            CreatedAt = edge.CreatedAt,
            ModifiedBy = edge.ModifiedBy,
            ModifiedAt = edge.ModifiedAt,
            ArchivedAt = edge.ArchivedAt
        };
    }
    
    /// <summary>
    /// Private method to facilitate boilerplate code for finding edges by ID or origindestination
    /// </summary>
    /// <param name="edgeId">The id whereby to fetch the edge</param>
    /// <param name="originId">The origin ID by which to fetch the edge if no ID</param>
    /// <param name="destinationId">The destination ID by which to fetch the edge if no ID</param>
    /// <param name="historical">Boolean indicating whether to look for a historical edge</param>
    /// <returns>The edge associated with the given id or origin/destination combo</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    private async Task<HistoricalEdge> FindEdge(
        long? edgeId, 
        long? originId, 
        long? destinationId
        )
    {
        if (edgeId == null && (originId == null || destinationId == null))
        {
            throw new KeyNotFoundException("Please supply either an edgeID or an originID and destinationID");
        }
        
        HistoricalEdge edge = null;

        // search for edge either by id or origin + destination
        if (edgeId != null)
        {
            edge = await _context.HistoricalEdges
                .Where(e => e.EdgeId == edgeId)
                .Where(e => e.Current)
                .FirstOrDefaultAsync();
        }
        else
        {
            edge = await _context.HistoricalEdges
                .Where(e => e.OriginId == originId && e.DestinationId == destinationId)
                .Where(e => e.Current)
                .FirstOrDefaultAsync();
        }

        // throw an error if edge not found
        if (edge == null)
        {
            if (edgeId != null)
            {
                throw new KeyNotFoundException($"Historical edge with id {edgeId} not found");
            }
            else
            {
                throw new KeyNotFoundException($"Historical edge with origin {originId} and destination {destinationId} not found");
            }
        }
        
        return edge;  
    }
}