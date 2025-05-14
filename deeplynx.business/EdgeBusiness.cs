using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class EdgeBusiness : IEdgeBusiness
{
    private readonly DeeplynxContext _context;

    public EdgeBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Edge>> GetAllEdges(long projectId, long? dataSourceId)
    {
        // base query object to get all edges
        var edgeQuery = _context.Edges.AsQueryable();
        
        // add filter for project
        edgeQuery = edgeQuery.Where(e => e.ProjectId == projectId && e.DataSourceId == dataSourceId);
        
        // add filter for datasource if specified
        if (dataSourceId.HasValue)
        {
            edgeQuery = edgeQuery.Where(e => e.DataSourceId == dataSourceId);
        }

        // execute query and return results
        return await edgeQuery.ToListAsync();
    }

    public async Task<Edge> GetEdge(long originId, long destinationId)
    {
        return await _context.Edges
            .FirstOrDefaultAsync(e => e.OriginId == originId && e.DestinationId == destinationId && e.DeletedAt == null);
    }

    public async Task<Edge> CreateEdge(long projectId, long dataSourceId, EdgeRequestDto dto)
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
        
        return edge;
    }

    public async Task<Edge> UpdateEdge(long originId, long destinationId, EdgeRequestDto dto)
    {
        var edge = await GetEdge(originId, destinationId);
        
        edge.Properties = dto.Properties?.ToString();
        edge.RelationshipId = dto.RelationshipId;
        edge.RelationshipName = dto.RelationshipName;
        edge.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        edge.ModifiedBy = null;  // TODO: Implement user ID here when JWT tokens are ready
        
        _context.Edges.Update(edge);
        await _context.SaveChangesAsync();
        
        return edge;
    }

    public async Task<bool> DeleteEdge(long originId, long destinationId)
    {
        var edge = await GetEdge(originId, destinationId);
        
        edge.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        _context.Edges.Update(edge);
        await _context.SaveChangesAsync();

        return true;
    }
}