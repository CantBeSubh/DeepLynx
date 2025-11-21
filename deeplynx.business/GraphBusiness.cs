using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace deeplynx.business;

public class GraphBusiness : IGraphBusiness
{
    private readonly ICacheBusiness _cacheBusiness;
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GraphBusiness" /> class.
    /// </summary>
    /// <param name="context">The database context used for the Graph operations.</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    /// <param name="eventBusiness">Used for logging events during create, update, and delete Operations.</param>
    public GraphBusiness(
        DeeplynxContext context, ICacheBusiness cacheBusiness, IEventBusiness eventBusiness)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
        _eventBusiness = eventBusiness;
    }

    /// <summary>
    ///     Retrieves all edges for a specific project and (optionally) datasource
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
        if (page < 1) throw new ArgumentException("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100) throw new ArgumentException("Page size must be between 1 and 100");

        var recordExists = await _context.Records.AnyAsync(record => record.Id == recordId);
        if (!recordExists) throw new KeyNotFoundException($"Record with id {recordId} not found");

        IQueryable<Edge> edgeQuery = _context.Edges
            .Include(e => e.Destination)
            .Include(e => e.Origin)
            .Include(e => e.Relationship);

        if (isOrigin)
            edgeQuery = edgeQuery.Where(e => e.OriginId == recordId);
        else
            edgeQuery = edgeQuery.Where(e => e.DestinationId == recordId);

        // TODO: Add this query back when we want to filter all record edges by user access

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

        if (hideArchived) edgeQuery = edgeQuery.Where(e => !e.IsArchived);

        return await edgeQuery
            .OrderBy(e => e.Id) // Important: Add consistent ordering for predictable pagination
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                Edge = e,
                RelatedRecord = isOrigin ? e.Destination : e.Origin
            })
            .Select(x => new RelatedRecordsResponseDto
            {
                RelatedRecordName = x.RelatedRecord.Name,
                RelatedRecordId = x.RelatedRecord.Id,
                RelatedRecordProjectId = x.RelatedRecord.ProjectId,
                RelationshipName = x.Edge.Relationship != null ? x.Edge.Relationship.Name : null
            }).ToListAsync();
    }

    /// <summary>
    ///     Gets related records up to 3 levels deep
    /// </summary>
    /// <param name="recordId">The record Id to start</param>
    /// <param name="userId">The user accessing this info</param>
    /// <param name="depth">How many relationships away the user wants</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<GraphResponse> GetGraphDataForRecord(long recordId, long userId, int depth)
    {
        if (depth > 3) throw new ArgumentException("Depth must be no more than 3");

        var rootRecord = await _context.Records.FindAsync(recordId);
        if (rootRecord == null) throw new KeyNotFoundException($"Record with id {recordId} not found");

        // find projects the user has access to
        var userProjectIds = await _context.Projects.Where(p =>
                p.ProjectMembers.Any(pm =>
                    pm.UserId == userId ||
                    (pm.GroupId.HasValue && pm.Group != null && pm.Group.Users.Any(u => u.Id == userId))
                ))
            .Select(p => p.Id)
            .ToListAsync();

        if (userProjectIds.Count == 0 || !userProjectIds.Contains(rootRecord.ProjectId))
            throw new AccessViolationException($"You do not have access to view record with id {recordId}");

        var nodes = new Dictionary<long, GraphNode>(); // Stores all unique nodes we discover
        var links = new List<GraphLink>(); // Stores all connections between nodes
        var visitedEdges = new HashSet<long>(); // Tracks which edges we've already processed (prevents duplicates)
        var visitedRecords = new HashSet<long>(); // Tracks which records we've already explored (prevents reprocessing)

        // Add the starting record as our root node
        nodes[recordId] = new GraphNode
        {
            Id = recordId,
            Label = rootRecord.Name,
            Type = "root" // root of the graph
        };

        // Start with just the root node to process
        var currentLevelRecordIds = new List<long> { recordId };

        for (var currentDepth = 0; currentDepth < depth; currentDepth++)
        {
            var nextLevelRecordIds = new List<long>();

            // Process each record at the current level
            foreach (var currentLevelRecordId in currentLevelRecordIds)
            {
                // Skip if we've already explored this record
                if (visitedRecords.Contains(currentLevelRecordId)) continue;

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
    ///     Gets all edges connected to a specific record from the database
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
                userProjectIds.Contains(e.Destination
                    .ProjectId) && // only edges that have destinations in user projects
                !e.IsArchived); // Only non-archived edges

        if (isOutgoing)
            query = query.Where(e => e.OriginId == recordId);
        else
            query = query.Where(e => e.DestinationId == recordId);

        return await query.ToListAsync();
    }

    /// <summary>
    ///     Processes a list of edges, adding new nodes and links to our graph data structures
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
            if (visitedEdges.Contains(edge.Id)) continue;

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
}