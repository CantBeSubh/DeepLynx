using System.ComponentModel.DataAnnotations;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

/// <summary>
///     Controller for managing edges.
/// </summary>
/// <remarks>
///     This controller provides endpoints to create, update, delete, and retrieve edge information.
/// </remarks>
[ApiController]
[Route("organizations/{organizationId}/projects/{projectId}/edges")]
[Authorize]
public class EdgeController : ControllerBase
{
    private readonly IEdgeBusiness _edgeBusiness;
    private readonly ILogger<EdgeController> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EdgeController" /> class
    /// </summary>
    /// <param name="edgeBusiness">The business logic interface for handling edge operations.</param>
    /// <param name="logger">Error/Info logging interface for database log table.</param>
    public EdgeController(IEdgeBusiness edgeBusiness, ILogger<EdgeController> logger)
    {
        _edgeBusiness = edgeBusiness;
        _logger = logger;
    }

    /// <summary>
    ///     Get All Edges
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project whose edges are to be retrieved</param>
    /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter edges</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
    /// <returns>A list of edges based on the applied filters.</returns>
    [HttpGet(Name = "api_get_all_edges")]
    public async Task<ActionResult<IEnumerable<EdgeResponseDto>>> GetAllEdges(
        long organizationId,
        long projectId,
        [FromQuery] long? dataSourceId = null,
        [FromQuery] bool hideArchived = true)
    {
        try
        {
            var edges = await _edgeBusiness.GetAllEdges(projectId, dataSourceId, hideArchived);
            return Ok(edges);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while listing all edges: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Get Edges by Record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record by which to filter edges</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
    /// <param name="isOrigin">Indicates whether to find where recordId is origin or not</param>
    /// <param name="page">Indicates the page number for pagination</param>
    /// <param name="pageSize">Indicates the page size for pagination</param>
    /// <returns>A list of related records based on edges.</returns>
    // TODO: move to RecordController?
    [HttpGet("records/{recordId}", Name = "api_get_edges_by_record")]
    public async Task<ActionResult<IEnumerable<RelatedRecordsResponseDto>>> GetEdgesByRecord(
        long organizationId,
        long projectId,
        long recordId,
        [FromQuery] bool isOrigin,
        [FromQuery] int page,
        [FromQuery] bool hideArchived = true,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var edges = await _edgeBusiness.GetEdgesByRecord(recordId, isOrigin, page, hideArchived, pageSize);
            return Ok(edges);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while listing edges by record: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Get Graph Data for Record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record for which to retrieve graph data</param>
    /// <param name="depth">The number of levels you want to search through</param>
    /// <returns>Graph data including nodes and edges.</returns>
    // TODO: move to RecordController?
    [HttpGet("graph/{recordId}", Name = "api_get_graph_data_for_record")]
    public async Task<ActionResult<GraphResponse>> GetGraphDataForRecord(
        long organizationId,
        long projectId,
        long recordId,
        [FromQuery] int depth)
    {
        try
        {
            var edges = await _edgeBusiness.GetGraphDataForRecord(recordId, UserContextStorage.UserId, depth);
            return Ok(edges);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while retrieving graph data: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Get an Edge
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="edgeId">The ID whereby to fetch the edge (if using ID-based lookup)</param>
    /// <param name="originId">The origin ID by which to fetch the edge (if using origin/destination lookup)</param>
    /// <param name="destinationId">The destination ID by which to fetch the edge (if using origin/destination lookup)</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
    /// <returns>The edge associated with the given id or origin/destination combo</returns>
    [HttpGet("edge", Name = "api_get_an_edge")]
    public async Task<ActionResult<EdgeResponseDto>> GetEdge(
        long organizationId,
        long projectId,
        [FromQuery] long? edgeId,
        [FromQuery] long? originId,
        [FromQuery] long? destinationId,
        [FromQuery] bool hideArchived = true)
    {
        try
        {
            var edge = await _edgeBusiness.GetEdge(projectId, edgeId, originId, destinationId, hideArchived);
            return Ok(edge);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while retrieving edge: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Create an Edge
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
    /// <param name="edge">The edge request data transfer object containing edge details</param>
    [HttpPost(Name = "api_create_an_edge")]
    public async Task<ActionResult<EdgeResponseDto>> CreateEdge(
        long organizationId,
        long projectId,
        [FromQuery] [Required] long dataSourceId,
        [FromBody] CreateEdgeRequestDto edge)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var createdEdge = await _edgeBusiness.CreateEdge(currentUserId, projectId, dataSourceId, edge);
            return Ok(createdEdge);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while creating edge: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Bulk Create Edges
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edges belong</param>
    /// <param name="dataSourceId">The ID of the data source to which the edges belong</param>
    /// <param name="edges">List of edge request data transfer objects containing edge details</param>
    [HttpPost("bulk", Name = "api_create_many_edges")]
    public async Task<ActionResult<List<EdgeResponseDto>>> BulkCreateEdges(
        long organizationId,
        long projectId,
        [FromQuery] [Required] long dataSourceId,
        [FromBody] List<CreateEdgeRequestDto> edges)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var createdEdges = await _edgeBusiness.BulkCreateEdges(currentUserId, projectId, dataSourceId, edges);
            return Ok(createdEdges);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while creating edges: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Update an Edge
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="dto">The edge request data transfer object containing updated edge details.</param>
    /// <param name="edgeId">The ID of the edge to update (if using ID-based lookup)</param>
    /// <param name="originId">The origin ID of the edge to update (if using origin/destination lookup)</param>
    /// <param name="destinationId">The destination ID of the edge (if using origin/destination lookup)</param>
    /// <returns>The updated edge response DTO with its details</returns>
    [HttpPut("edge", Name = "api_update_an_edge")]
    public async Task<ActionResult<EdgeResponseDto>> UpdateEdge(
        long organizationId,
        long projectId,
        [FromBody] UpdateEdgeRequestDto dto,
        [FromQuery] long? edgeId,
        [FromQuery] long? originId,
        [FromQuery] long? destinationId)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var updatedEdge =
                await _edgeBusiness.UpdateEdge(currentUserId, projectId, dto, edgeId, originId, destinationId);
            return Ok(updatedEdge);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while updating edge: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Delete an Edge
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="edgeId">The ID of the edge to delete (if using ID-based lookup)</param>
    /// <param name="originId">The origin ID of the edge to delete (if using origin/destination lookup)</param>
    /// <param name="destinationId">The destination ID of the edge (if using origin/destination lookup)</param>
    /// <param name="force">Force delete even if archived</param>
    /// <returns>A message stating the edge was successfully deleted.</returns>
    [HttpDelete("edge", Name = "api_delete_an_edge")]
    public async Task<IActionResult> DeleteEdge(
        long organizationId,
        long projectId,
        [FromQuery] long? edgeId,
        [FromQuery] long? originId,
        [FromQuery] long? destinationId,
        [FromQuery] bool force = false)
    {
        try
        {
            edgeId = await _edgeBusiness.DeleteEdge(projectId, edgeId, originId, destinationId);
            return Ok(new { message = $"Deleted edge {edgeId}" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting edge: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Archive or Unarchive an Edge
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="edgeId">The ID of the edge to archive/unarchive (if using ID-based lookup)</param>
    /// <param name="originId">The origin ID of the edge to archive/unarchive (if using origin/destination lookup)</param>
    /// <param name="destinationId">The destination ID of the edge (if using origin/destination lookup)</param>
    /// <param name="archive">True to archive the edge, false to unarchive it.</param>
    /// <returns>A message stating the edge was successfully archived or unarchived.</returns>
    [HttpPatch("edge", Name = "api_archive_edge")]
    public async Task<IActionResult> ArchiveEdge(
        long organizationId,
        long projectId,
        [FromQuery] long? edgeId,
        [FromQuery] long? originId,
        [FromQuery] long? destinationId,
        [FromQuery] bool archive)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            if (archive)
            {
                edgeId = await _edgeBusiness.ArchiveEdge(currentUserId, projectId, edgeId, originId, destinationId);
                return Ok(new { message = $"Archived edge {edgeId}" });
            }

            edgeId = await _edgeBusiness.UnarchiveEdge(currentUserId, projectId, edgeId, originId, destinationId);
            return Ok(new { message = $"Unarchived edge {edgeId}" });
        }
        catch (Exception exc)
        {
            var action = archive ? "archiving" : "unarchiving";
            var message = $"An error occurred while {action} edge: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}