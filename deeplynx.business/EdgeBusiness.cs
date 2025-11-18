using System.ComponentModel.DataAnnotations;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

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
    /// Gets related records up to 3 levels deep
    /// </summary>
    /// <param name="recordId">The record Id to start</param>
    /// <param name="userId">The user accessing this info</param>
    /// <param name="depth">How many relationships away the user wants</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<GraphResponse> GetGraphDataForRecord(long recordId, long userId, int depth)
    {
        if (depth > 3)
        {
            throw new ArgumentException("Depth must be no more than 3");
        }
        var rootRecord = await _context.Records.FindAsync(recordId);
        if (rootRecord == null)
        {
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        }
        
        // find projects the user has access to
        var userProjectIds = await _context.Projects.Where(p => 
                p.ProjectMembers.Any(pm => 
                    pm.UserId == userId ||
                    (pm.GroupId.HasValue && pm.Group != null && pm.Group.Users.Any(u => u.Id == userId))
                ))
            .Select(p => p.Id)
            .ToListAsync();
        
        if (userProjectIds.Count == 0 || !userProjectIds.Contains(rootRecord.ProjectId))
        {
            throw new AccessViolationException($"You do not have access to view record with id {recordId}");
        }
        
        var nodes = new Dictionary<long, GraphNode>();  // Stores all unique nodes we discover
        var links = new List<GraphLink>();              // Stores all connections between nodes
        var visitedEdges = new HashSet<long>();         // Tracks which edges we've already processed (prevents duplicates)
        var visitedRecords = new HashSet<long>();         // Tracks which records we've already explored (prevents reprocessing)

        // Add the starting record as our root node
        nodes[recordId] = new GraphNode
        {
            Id = recordId,
            Label = rootRecord.Name,
            Type = "root"  // root of the graph
        };

        // Start with just the root node to process
        var currentLevelRecordIds = new List<long> { recordId };
        
        for (int currentDepth = 0; currentDepth < depth; currentDepth++)
        {
            var nextLevelRecordIds = new List<long>();

            // Process each record at the current level
            foreach (var currentLevelRecordId in currentLevelRecordIds)
            {
                // Skip if we've already explored this record
                if (visitedRecords.Contains(currentLevelRecordId))
                {
                    continue;
                }
                
                visitedRecords.Add(currentLevelRecordId);

                // Get all connections FROM this record (outgoing edges)
                var outgoingEdges = await GetGraphEdges(currentLevelRecordId, userProjectIds, true);
                
                // Get all connections TO this record (incoming edges)
                var incomingEdges = await GetGraphEdges(currentLevelRecordId, userProjectIds, false);

                // add links and nodes to the graph for outgoing and incoming
                ProcessEdges(outgoingEdges, nodes, links, visitedEdges, nextLevelRecordIds, true);
                ProcessEdges(incomingEdges, nodes, links, visitedEdges, nextLevelRecordIds, false);
            }

            // Move next level records to current
            currentLevelRecordIds = nextLevelRecordIds;
        }
        
        return new GraphResponse
        {
            Nodes = nodes.Values.ToList(),
            Links = links
        };
    }

    /// <summary>
    /// Gets all edges connected to a specific record from the database
    /// </summary>
    /// <param name="recordId">The ID of the record to get edges for</param>
    /// <param name="userProjectIds">The ID of the projects the user has access to</param>
    /// <param name="isOutgoing">True for edges going OUT from this record, False for edges coming IN to this record</param>
    /// <returns>A list of edges with their related data (origin, destination, relationship) loaded</returns>
    private async Task<List<Edge>> GetGraphEdges(long recordId, List<long> userProjectIds, bool isOutgoing)
    {
        // Start building our database query
        var query = _context.Edges
            .Include(e => e.Origin)      
            .Include(e => e.Destination)  
            .Include(e => e.Relationship)
            .Where(e => 
                userProjectIds.Contains(e.ProjectId) && // only edges in user projects
                userProjectIds.Contains(e.Origin.ProjectId) && // only edges that have origins in user projects
                userProjectIds.Contains(e.Destination.ProjectId) && // only edges that have destinations in user projects
                !e.IsArchived);    // Only non-archived edges
        
        if (isOutgoing)
        {
            query = query.Where(e => e.OriginId == recordId);
        }
        else
        {
            query = query.Where(e => e.DestinationId == recordId);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Processes a list of edges, adding new nodes and links to our graph data structures
    /// </summary>
    /// <param name="edges">The edges to process</param>
    /// <param name="nodes">Dictionary of all nodes in the graph (we add to this)</param>
    /// <param name="links">List of all links in the graph (we add to this)</param>
    /// <param name="visitedEdges">Set of edge IDs we've already processed (prevents duplicates)</param>
    /// <param name="nextLevelRecords">List to add newly discovered node IDs to (for next depth level)</param>
    /// <param name="isOutgoing">True if these are outgoing edges, False if incoming</param>
    private void ProcessEdges(
        List<Edge> edges,
        Dictionary<long, GraphNode> nodes,
        List<GraphLink> links,
        HashSet<long> visitedEdges,
        List<long> nextLevelRecords,
        bool isOutgoing)
    {
        foreach (var edge in edges)
        {
            // Skip if edge already visited
            if (visitedEdges.Contains(edge.Id))
            {
                continue;
            }
            
            visitedEdges.Add(edge.Id);

            // Figure out which record is on the other side of this edge
            var connectedRecordId = isOutgoing ? edge.DestinationId : edge.OriginId;
            var connectedRecord = isOutgoing ? edge.Destination : edge.Origin;

            // If this is a new node, add it to the graph
            if (!nodes.ContainsKey(connectedRecordId))
            {
                nodes[connectedRecordId] = new GraphNode
                {
                    Id = connectedRecordId,
                    Label = connectedRecord.Name,
                    Type = "node"
                };

                // Add this node to the list of nodes to explore in the next depth level
                nextLevelRecords.Add(connectedRecordId);
            }

            // Add the link between nodes to the graph
            // Note: we always store Source -> Target in the original edge direction
            links.Add(new GraphLink
            {
                Source = edge.OriginId,   
                Target = edge.DestinationId,   
                RelationshipId = edge.RelationshipId,
                RelationshipName = edge.Relationship?.Name,
                EdgeId = edge.Id
            });
        }
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
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
    /// <param name="dto">The edge request data transfer object containing edge details</param>
    /// <returns>The created edge response DTO with saved details.</returns>
    public async Task<EdgeResponseDto> CreateEdge(
        long currentUserId,
        long projectId,
        long dataSourceId,
        CreateEdgeRequestDto dto)
    {
        if (!dto.OriginId.HasValue || !dto.DestinationId.HasValue)
        {
            throw new ValidationException("Origin and/or Destination IDs are missing or invalid.");
        }
        
        if (dto.OriginId == dto.DestinationId)
        {
            throw new ValidationException("Destination and origin IDs cannot be the same");
        }
        
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        await ExistenceHelper.EnsureDataSourceExistsForProjectAsync(_context,dataSourceId, projectId);
        
        var originRecordExists = _context.Records.Any(r => r.Id == dto.OriginId); 
        if (!originRecordExists)
        {
            throw new KeyNotFoundException($"Origin record with id {dto.OriginId} not found");
        }
        
        var destinationRecordExists = _context.Records.Any(r => r.Id == dto.DestinationId);
        if (!destinationRecordExists)
        {
            throw new KeyNotFoundException($"Destination record with id {dto.DestinationId} not found");
        }
        
        var edge = new Edge
        {
            OriginId = dto.OriginId.Value,
            DestinationId = dto.DestinationId.Value,
            ProjectId = projectId,
            DataSourceId = dataSourceId,
            RelationshipId = dto.RelationshipId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = currentUserId
        };

        _context.Edges.Add(edge);
        await _context.SaveChangesAsync();

        // log edge create event
        await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
        {
            Operation = "create",
            EntityType = "edge",
            EntityId = edge.Id,
            DataSourceId = edge.DataSourceId,
            Properties = JsonSerializer.Serialize(new 
            { 
                origin = edge.OriginId,
                destination = edge.DestinationId
            }), // TODO: Determine the extent of data edge properties need
        }, null, projectId);

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
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
    /// <param name="edges">The edge request data transfer object containing edge details</param>
    /// <returns>The created edge response DTO with saved details.</returns>
    public async Task<List<EdgeResponseDto>> BulkCreateEdges(
        long currentUserId,
        long projectId,
        long dataSourceId,
        List<CreateEdgeRequestDto> edges)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        await ExistenceHelper.EnsureDataSourceExistsForProjectAsync(_context, dataSourceId, projectId);

        // Bulk insert into edges; if there is an origin/destination collision, update relationship ID
        var sql = @"
            INSERT INTO deeplynx.edges (project_id, data_source_id, origin_id, destination_id, relationship_id, last_updated_at, last_updated_by, is_archived)
            VALUES {0}
            ON CONFLICT (project_id, origin_id, destination_id) DO UPDATE SET
                relationship_id = COALESCE(EXCLUDED.relationship_id, edges.relationship_id),
                last_updated_at = @now,
                last_updated_by = @lastUpdatedBy
            RETURNING *;
        ";

        // establish "constant" parameters
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@projectId", projectId),
            new NpgsqlParameter("@dataSourceId", dataSourceId),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)),
            new NpgsqlParameter("@lastUpdatedBy", currentUserId)
        };

        // establish "dynamic" parameters (new for each dto in the list)
        parameters.AddRange(edges.SelectMany((dto, i) =>
        {
            if (!dto.DestinationId.HasValue || !dto.OriginId.HasValue)
            {
                throw new ValidationException("Destination and origin IDs are missing or invalid.");
            }

            if (dto.DestinationId == dto.OriginId)
            {
                throw new ValidationException("Destination and origin IDs cannot be the same");
            }
            
            var originRecordExists = _context.Records.Any(r => r.Id == dto.OriginId); 
            if (!originRecordExists)
            {
                throw new KeyNotFoundException($"Origin record with id {dto.OriginId} not found");
            }
        
            var destinationRecordExists = _context.Records.Any(r => r.Id == dto.DestinationId);
            if (!destinationRecordExists)
            {
                throw new KeyNotFoundException($"Destination record with id {dto.DestinationId} not found");
            }
            
            return new[]
            {
                new NpgsqlParameter($"@p{i}_orig", dto.OriginId),
                new NpgsqlParameter($"@p{i}_dest", dto.DestinationId),
                new NpgsqlParameter($"@p{i}_rel", (object?)dto.RelationshipId ?? DBNull.Value),
            };
        }));

        // stringify the params and comma separate them
        var valueTuples = string.Join(", ", edges.Select((dto, i) =>
            $"(@projectId, @dataSourceId, @p{i}_orig, @p{i}_dest, @p{i}_rel, @now, @lastUpdatedBy, false)"));

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
                Operation = "create",
                EntityType = "edge",
                EntityId = newEdge.Id,
                DataSourceId = newEdge.DataSourceId,
                Properties = "{}", // TODO: Determine the extent of data edge properties need
            });
        }
        await _eventBusiness.BulkCreateEvents(currentUserId, events, null, projectId);

        return result;
    }

    /// <summary>
    /// Updates an existing edge by its ID or origin/destination.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">The ID of the project to which the edge belongs.</param>
    /// <param name="dto">The edge request data transfer object containing updated edge details.</param>
    /// <param name="edgeId">The ID of the edge to update</param>
    /// <param name="originId">The origin ID of the edge to update if edgeID is not present.</param>
    /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
    /// <returns>The updated edge response DTO with its details</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<EdgeResponseDto> UpdateEdge(
        long currentUserId,
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
        edge.LastUpdatedBy = currentUserId;

        if (edge.OriginId == edge.DestinationId)
        {
            throw new ValidationException("Destination and origin Ids can not be the same.");
        }
        
        _context.Edges.Update(edge);
        await _context.SaveChangesAsync();

        // log edge update event
        await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
        {
            Operation = "update",
            EntityType = "edge",
            EntityId = edge.Id,
            DataSourceId = edge.DataSourceId,
            Properties = "{}", // TODO: Determine the extent of data edge properties need
        }, null, projectId);

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
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">The ID of the project to which the edge belongs.</param>
    /// <param name="edgeId">The ID of the edge to archive</param>
    /// <param name="originId">The origin ID of the edge to archive if edgeID is not present.</param>
    /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
    /// <returns>The ID of the edge that was archived.</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<long> ArchiveEdge(
        long currentUserId,
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
        edge.LastUpdatedBy = currentUserId;
        await _context.SaveChangesAsync();

        // Log Edge soft Delete Event
        await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
        {
            Operation = "delete",
            EntityType = "edge",
            EntityId = edgeId,
            DataSourceId = edge.DataSourceId,
            Properties = "{}", // TODO: Determine the extent of data edge properties need
        }, null, projectId);

        return edge.Id;
    }

    /// <summary>
    /// Unarchives a specific edge by its ID or origin/destination.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">The ID of the project to which the edge belongs.</param>
    /// <param name="edgeId">The ID of the edge to unarchive</param>
    /// <param name="originId">The origin ID of the edge to unarchive if edgeID is not present.</param>
    /// <param name="destinationId">The destination ID of the edge to unarchive if edgeID is not present.</param>
    /// <returns>The ID of the edge that was unarchived.</returns>
    /// <exception cref="KeyNotFoundException">Returned if edge not found or if ids missing</exception>
    public async Task<long> UnarchiveEdge(
        long currentUserId,
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
        edge.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        edge.LastUpdatedBy = currentUserId;
        await _context.SaveChangesAsync();
        
        await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
        {
            Operation = "unarchive",
            EntityType = "edge",
            EntityId = edgeId,
            DataSourceId = edge.DataSourceId,
            Properties = "{}", // TODO: Determine the extent of data edge properties need
        }, null, projectId);

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