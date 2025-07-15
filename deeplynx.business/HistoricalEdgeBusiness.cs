using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class HistoricalEdgeBusiness : IHistoricalEdgeBusiness
{
    private readonly DeeplynxContext _context;

    public HistoricalEdgeBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HistoricalEdgeResponseDto>> GetAllHistoricalEdges(
        long projectId,
        long? dataSourceId = null,
        DateTime? poinInTime = null,
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
        if (poinInTime.HasValue && !current)
        {
            // compare timestamp to the most recent update
            edgeQuery = edgeQuery
                .Where(e => e.LastUpdatedAt <= poinInTime)
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

    public async Task<IEnumerable<HistoricalEdgeResponseDto>> GetHistoryForEdge(long edgeId)
    {
        return await _context.HistoricalEdges
            .Where(e => e.EdgeId == edgeId)
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

    public async Task<HistoricalEdgeResponseDto> GetHistoricalEdge(
        long edgeId,
        DateTime? pointInTime,
        bool hideArchived = true,
        bool current = true)
    {
        var edgeQuery = _context.HistoricalEdges
            .Where(e => e.EdgeId == edgeId);

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
            throw new KeyNotFoundException($"Edge with id {edgeId} not found at point in time {pointInTime}.");
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

    public async Task<bool> CreateHistoricalEdge(long edgeId)
    {
        // insert the appropriate data using insert into select
        // due to the complexity of the query, execute the query
        // using raw SQL instead of via entity framework
        var query = @"
            INSERT INTO deeplynx.historical_edges (
	            edge_id, origin_id, destination_id, mapping_id,
	            relationship_id, data_source_id, project_id,
	            created_at, created_by, current, 
	            relationship_name, data_source_name, project_name)
            SELECT e.id, e.origin_id, e.destination_id, e.mapping_id,
	            e.relationship_id, e.data_source_id, e.project_id,
	            e.created_at, e.created_by, TRUE, 
	            r.name, d.name, p.name
            FROM deeplynx.edges e
            LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
            JOIN deeplynx.data_sources d ON d.id = e.data_source_id
            JOIN deeplynx.projects p ON p.id = e.project_id
            WHERE e.id = @EdgeId;";
        
        var edgeIdParam = new Npgsql.NpgsqlParameter("@EdgeId", edgeId);
        
        var created = await _context.Database.ExecuteSqlRawAsync(query, edgeIdParam);

        if (created == 0) // if 0 edges were created, assume a failure
        {
            throw new Exception($"Unable to create historical edge with id {edgeId}");
        }
        
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> UpdateHistoricalEdge(long edgeId)
    {
        // set all previous instances of "current" for this edge id to false
        await _context.HistoricalEdges
            .Where(e => e.EdgeId == edgeId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Current, false));

        // insert the appropriate data using insert into select
        // due to the complexity of the query, execute the query
        // using raw SQL instead of via entity framework
        var query = @"
            INSERT INTO deeplynx.historical_edges (
	            edge_id, origin_id, destination_id, mapping_id,
	            relationship_id, data_source_id, project_id,
	            created_at, created_by, modified_at, modified_by, 
                current, relationship_name, data_source_name, project_name)
            SELECT e.id, e.origin_id, e.destination_id, e.mapping_id,
	            e.relationship_id, e.data_source_id, e.project_id,
	            e.created_at, e.created_by, e.modified_at, e.modified_by, 
	            TRUE, r.name, d.name, p.name
            FROM deeplynx.edges e
            LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
            JOIN deeplynx.data_sources d ON d.id = e.data_source_id
            JOIN deeplynx.projects p ON p.id = e.project_id
            WHERE e.id = @EdgeId;";
        
        var edgeIdParam = new Npgsql.NpgsqlParameter("@EdgeId", edgeId);
        
        var updated = await _context.Database.ExecuteSqlRawAsync(query, edgeIdParam);

        if (updated == 0) // if 0 edges were updated, assume a failure
        {
            throw new Exception($"Unable to update historical edge with id {edgeId}");
        }
        
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> ArchiveHistoricalEdge(long edgeId)
    {
        // set all previous instances of "current" for this edge id to false
        await _context.HistoricalEdges
            .Where(e => e.EdgeId == edgeId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Current, false));

        // insert the appropriate data using insert into select
        // due to the complexity of the query, execute the query
        // using raw SQL instead of via entity framework
        var query = @"
            INSERT INTO deeplynx.historical_edges (
	            edge_id, origin_id, destination_id, mapping_id,
	            relationship_id, data_source_id, project_id,
	            created_at, created_by, modified_at, 
	            modified_by, archived_at, current, 
	            relationship_name, data_source_name, project_name)
            SELECT e.id, e.origin_id, e.destination_id, e.mapping_id,
	            e.relationship_id, e.data_source_id, e.project_id,
	            e.created_at, e.created_by, e.modified_at, 
	            e.modified_by, e.archived_at, TRUE, 
	            r.name, d.name, p.name
            FROM deeplynx.edges e
            LEFT JOIN deeplynx.relationships r ON r.id = e.relationship_id
            JOIN deeplynx.data_sources d ON d.id = e.data_source_id
            JOIN deeplynx.projects p ON p.id = e.project_id
            WHERE e.id = @EdgeId;";
        
        var edgeIdParam = new Npgsql.NpgsqlParameter("@EdgeId", edgeId);
        
        var archived = await _context.Database.ExecuteSqlRawAsync(query, edgeIdParam);

        if (archived == 0) // if 0 edges were archived, assume a failure
        {
            throw new Exception($"Unable to archive historical edge with id {edgeId}");
        }
        
        await _context.SaveChangesAsync();
        
        return true;
    }
}