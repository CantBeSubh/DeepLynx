using System.ComponentModel.DataAnnotations;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace deeplynx.business;

public class EdgeBusiness : IEdgeBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;
    private readonly ICacheBusiness _cacheBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the edge operations.</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    /// <param name="historicalEdgeBusiness">Passed in context of historical edge objects.</param>
    /// <param name="eventBusiness">Used for logging events during create, update, and delete Operations.</param>
    public EdgeBusiness(
        DeeplynxContext context, ICacheBusiness cacheBusiness, IEventBusiness eventBusiness)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
        _eventBusiness = eventBusiness;
    }

    /// <summary>
    /// Retrieves all edges for a specific project and (optionally) datasource
    /// </summary>
    /// <param name="projectId">The ID of the project whose edges are to be retrieved</param>
    /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter edges</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result</param>
    /// <returns>A list of edges based on the applied filters.</returns>
    public async Task<List<EdgeResponseDto>> GetAllEdges(
        long projectId,
        long? dataSourceId,
        bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);
        var edgeQuery = _context.Edges
            .Where(e => e.ProjectId == projectId);

        if (hideArchived)
        {
            edgeQuery = edgeQuery.Where(e => e.IsArchived == false);
        }

        var edges = await edgeQuery.ToListAsync();

        return edges
            .Select(e => new EdgeResponseDto()
            {
                Id = e.Id,
                OriginId = e.OriginId,
                DestinationId = e.DestinationId,
                RelationshipId = e.RelationshipId,
                DataSourceId = e.DataSourceId,
                ProjectId = e.ProjectId,
                LastUpdatedAt = e.LastUpdatedAt,
                LastUpdatedBy = e.LastUpdatedBy,
                IsArchived = e.IsArchived,
            }).ToList();
    }

    /// <summary>
    /// Retrieves all edges for a specific project and (optionally) datasource
    /// </summary>
    /// <param name="recordId">The ID of the record by which to filter edges</param>
    /// <param name="isOrigin">Indicates whether to find where recordId is origin or not</param>
    /// <param name="page">The ID of the record by which to filter edges</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result</param>
    /// <param name="pageSize">Max size of list to return</param>
    /// <returns>A list of edges based on the applied filters.</returns>
    public async Task<List<RelatedRecordsResponseDto>> GetEdgesByRecord(
        long recordId,
        bool isOrigin,
        int page,
        bool hideArchived,
        int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page must be greater than 0");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100");
        }

        var recordExists = await _context.Records.AnyAsync(record => record.Id == recordId);
        if (!recordExists)
        {
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        }

        IQueryable<Edge> edgeQuery = _context.Edges
            .Include(e => e.Destination)
            .Include(e => e.Origin)
            .Include(e => e.Relationship);

        if (isOrigin)
        {
            edgeQuery = edgeQuery.Where(e => e.OriginId == recordId);
        }
        else
        {
            edgeQuery = edgeQuery.Where(e => e.DestinationId == recordId);
        }

        // Todo: Add this query back when we want to filter all record edges by user access

        // var userProjectIds = await _context.Projects.Where(p => 
        //     p.ProjectMembers.Any(pm => 
        //         pm.UserId == userId ||
        //         (pm.GroupId.HasValue && pm.Group != null && pm.Group.Users.Any(u => u.Id == userId))
        //     ))
        //     .Select(p => p.Id)
        //     .ToListAsync();
        //
        // if (!userProjectIds.Any())
        // {
        //     return new List<EdgeResponseDto>();
        // }
        //
        // var edgeQuery = _context.Edges
        //     .Where(e =>
        //         userProjectIds.Contains(e.ProjectId) && 
        //         (e.OriginId == recordId || e.DestinationId == recordId) && 
        //         userProjectIds.Contains(e.Origin.ProjectId) && 
        //         userProjectIds.Contains(e.Destination.ProjectId));

        if (hideArchived)
        {
            edgeQuery = edgeQuery.Where(e => !e.IsArchived);
        }

        return await edgeQuery
            .OrderBy(e => e.Id) // Important: Add consistent ordering for predictable pagination
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                Edge = e,
                RelatedRecord = isOrigin ? e.Destination : e.Origin
            })
            .Select(x => new RelatedRecordsResponseDto()
            {
                RelatedRecordName = x.RelatedRecord.Name,
                RelatedRecordId = x.RelatedRecord.Id,
                RelatedRecordProjectId = x.RelatedRecord.ProjectId,
                RelationshipName = x.Edge.Relationship != null ? x.Edge.Relationship.Name : null,
            }).ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific edge by its origin and destination IDs
    /// OR Retrieves an edge by its id
    /// </summary>
    /// <param name="projectId">The project of the edge to retrieve</param>
    /// <param name="edgeId">The id whereby to fetch the edge</param>
    /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
    /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result</param>
    /// <returns>The edge associated with the given id or origin/destination combo</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or is archived</exception>
    public async Task<EdgeResponseDto> GetEdge(
        long projectId,
        long? edgeId,
        long? originId,
        long? destinationId,
        bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);

        var edge = await FindEdge(edgeId, originId, destinationId);

        if (edge == null)
        {
            throw new KeyNotFoundException($"Edge with id {edgeId} not found");
        }

        if (hideArchived && edge.IsArchived)
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
            LastUpdatedAt = edge.LastUpdatedAt,
            LastUpdatedBy = edge.LastUpdatedBy,
            IsArchived = edge.IsArchived,
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
        CreateEdgeRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        await ExistenceHelper.EnsureDataSourceExistsForProjectAsync(_context, dataSourceId, projectId);

        if (!dto.OriginId.HasValue || !dto.DestinationId.HasValue)
        {
            throw new ValidationException("Origin and/or Destination IDs are missing or invalid.");
        }

        var edge = new Edge
        {
            OriginId = dto.OriginId.Value,
            DestinationId = dto.DestinationId.Value,
            ProjectId = projectId,
            DataSourceId = dataSourceId,
            RelationshipId = dto.RelationshipId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        };

        _context.Edges.Add(edge);
        await _context.SaveChangesAsync();

        // log edge create event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "create",
            EntityType = "edge",
            EntityId = edge.Id,
            DataSourceId = edge.DataSourceId,
            Properties = "{}", // TODO: Determine the extent of data edge properties need
            LastUpdatedBy = "" // TODO: Implement user ID here when JWT tokens are ready
        });

        return new EdgeResponseDto
        {
            Id = edge.Id,
            OriginId = edge.OriginId,
            DestinationId = edge.DestinationId,
            RelationshipId = edge.RelationshipId,
            DataSourceId = edge.DataSourceId,
            ProjectId = edge.ProjectId,
            LastUpdatedAt = edge.LastUpdatedAt,
            LastUpdatedBy = edge.LastUpdatedBy
        };
    }

    /// <summary>
    /// Asynchronously creates new edges for a specified project.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
    /// <param name="edges">The edge request data transfer object containing edge details</param>
    /// <returns>The created edge response DTO with saved details.</returns>
    public async Task<List<EdgeResponseDto>> BulkCreateEdges(
        long projectId,
        long dataSourceId,
        List<CreateEdgeRequestDto> edges)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        await ExistenceHelper.EnsureDataSourceExistsForProjectAsync(_context, dataSourceId, projectId);

        // Bulk insert into edges; if there is an origin/destination collision, update relationship ID
        var sql = @"
            INSERT INTO deeplynx.edges (project_id, data_source_id, origin_id, destination_id, relationship_id, last_updated_at,is_archived)
            VALUES {0}
            ON CONFLICT (project_id, origin_id, destination_id) DO UPDATE SET
                relationship_id = COALESCE(EXCLUDED.relationship_id, edges.relationship_id),
                last_updated_at = @now
            RETURNING *;
        ";

        // establish "constant" parameters
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@projectId", projectId),
            new NpgsqlParameter("@dataSourceId", dataSourceId),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
        };

        // establish "dynamic" parameters (new for each dto in the list)
        parameters.AddRange(edges.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@p{i}_orig", dto.OriginId),
            new NpgsqlParameter($"@p{i}_dest", dto.DestinationId),
            new NpgsqlParameter($"@p{i}_rel", (object?)dto.RelationshipId ?? DBNull.Value),
        }));

        // stringify the params and comma separate them
        var valueTuples = string.Join(", ", edges.Select((dto, i) =>
            $"(@projectId, @dataSourceId, @p{i}_orig, @p{i}_dest, @p{i}_rel, @now, false)"));

        // put everything together and execute the query
        sql = string.Format(sql, valueTuples);

        // returns the resulting upserted classes
        var result = await _context.Database
            .SqlQueryRaw<EdgeResponseDto>(sql, parameters.ToArray())
            .ToListAsync();

        // log edge create event for each create
        var events = new List<CreateEventRequestDto> { };
        foreach (var newEdge in result)
        {
            events.Add(new CreateEventRequestDto
            {
                ProjectId = projectId,
                Operation = "create",
                EntityType = "edge",
                EntityId = newEdge.Id,
                DataSourceId = newEdge.DataSourceId,
                Properties = "{}", // TODO: Determine the extent of data edge properties need
                LastUpdatedBy = "" // TODO: Implement user ID here when JWT tokens are ready
            });
        }
        await _eventBusiness.BulkCreateEvents(projectId, events);

        return result;
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
        UpdateEdgeRequestDto dto,
        long? edgeId,
        long? originId,
        long? destinationId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        // find edge and perform error handling if not found
        Edge edge = await FindEdge(edgeId, originId, destinationId);
        if (edge == null || edge.ProjectId != projectId || edge.IsArchived)
        {
            throw new KeyNotFoundException("Edge may have been moved or deleted.");
        }

        edge.OriginId = dto.OriginId ?? edge.OriginId;
        edge.DestinationId = dto.DestinationId ?? edge.DestinationId;
        edge.RelationshipId = dto.RelationshipId ?? edge.RelationshipId;
        edge.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        edge.LastUpdatedBy = null;  // TODO: Implement user ID here when JWT tokens are ready

        _context.Edges.Update(edge);
        await _context.SaveChangesAsync();

        // log edge update event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "update",
            EntityType = "edge",
            EntityId = edge.Id,
            DataSourceId = edge.DataSourceId,
            Properties = "{}", // TODO: Determine the extent of data edge properties need
            LastUpdatedBy = "" // TODO: add username when JWT are implemented
        });

        return new EdgeResponseDto
        {
            Id = edge.Id,
            OriginId = edge.OriginId,
            DestinationId = edge.DestinationId,
            RelationshipId = edge.RelationshipId,
            DataSourceId = edge.DataSourceId,
            ProjectId = edge.ProjectId,
            LastUpdatedAt = edge.LastUpdatedAt,
            LastUpdatedBy = edge.LastUpdatedBy,
            IsArchived = edge.IsArchived,
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
    /// TODO: return warning that historical data will be entirely wiped with this action
    public async Task<long> DeleteEdge(
        long projectId,
        long? edgeId,
        long? originId,
        long? destinationId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        // find edge and perform error handling if not found
        Edge edge = await FindEdge(edgeId, originId, destinationId);
        if (edge == null || edge.ProjectId != projectId)
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
    /// <returns>The ID of the edge that was archived.</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<long> ArchiveEdge(
        long projectId,
        long? edgeId,
        long? originId,
        long? destinationId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        // find edge and perform error handling if not found
        Edge edge = await FindEdge(edgeId, originId, destinationId);
        if (edge == null || edge.ProjectId != projectId || edge.IsArchived)
            throw new KeyNotFoundException("Edge may have been moved, archived or deleted.");

        edge.IsArchived = true;
        edge.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        await _context.SaveChangesAsync();

        // Log Edge soft Delete Event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "delete",
            EntityType = "edge",
            EntityId = edgeId,
            DataSourceId = edge.DataSourceId,
            Properties = "{}", // TODO: Determine the extent of data edge properties need
            LastUpdatedBy = "" // TODO: Implement user ID here when JWT tokens are ready
        });

        return edge.Id;
    }

    /// <summary>
    /// Unarchives a specific edge by its ID or origin/destination.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the edge belongs.</param>
    /// <param name="edgeId">The ID of the edge to unarchive</param>
    /// <param name="originId">The origin ID of the edge to unarchive if edgeID is not present.</param>
    /// <param name="destinationId">The destination ID of the edge to unarchive if edgeID is not present.</param>
    /// <returns>The ID of the edge that was unarchived.</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<long> UnarchiveEdge(
        long projectId,
        long? edgeId,
        long? originId,
        long? destinationId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        // find edge and perform error handling if not found
        Edge edge = await FindEdge(edgeId, originId, destinationId);
        if (edge == null || edge.ProjectId != projectId || !edge.IsArchived)
            throw new KeyNotFoundException("Edge to unarchive not found or is not archived.");

        edge.IsArchived = false;
        _context.Edges.Update(edge);
        await _context.SaveChangesAsync();

        return edge.Id;
    }

    /// <summary>
    /// Private method to facilitate boilerplate code for finding edges by ID or origin/destination
    /// </summary>
    /// <param name="edgeId">The id whereby to fetch the edge</param>
    /// <param name="originId">The origin ID by which to fetch the edge if no ID</param>
    /// <param name="destinationId">The destination ID by which to fetch the edge if no ID</param>
    /// <returns>The edge associated with the given id or origin/destination combo</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    private async Task<Edge> FindEdge(
        long? edgeId,
        long? originId,
        long? destinationId
        )
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
                .Where(e => e.Id == edgeId)
                .FirstOrDefaultAsync();
        }
        else
        {
            edge = await _context.Edges
                .Where(e => e.OriginId == originId && e.DestinationId == destinationId)
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
    /// Determine if datasource exists
    /// </summary>
    /// <param name="datasourceId">The ID of the datasource we are searching for</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
    /// <returns>Throws error if datasource does not exist</returns>
    private void DoesDataSourceExist(long datasourceId, bool hideArchived = true)
    {
        var datasource = hideArchived ? _context.DataSources.Any(p => p.Id == datasourceId && !p.IsArchived)
                : _context.DataSources.Any(p => p.Id == datasourceId);
        if (!datasource)
        {
            throw new KeyNotFoundException($"Datasource with id {datasourceId} not found");
        }
    }
}