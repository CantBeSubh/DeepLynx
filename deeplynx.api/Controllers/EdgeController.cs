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
    ///     Get an Edge by ID
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="edgeId">The ID of the edge to retrieve</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
    /// <returns>The edge associated with the given ID</returns>
    [HttpGet("{edgeId:long}", Name = "api_get_edge_by_id")]
    public async Task<ActionResult<EdgeResponseDto>> GetEdgeById(
        long organizationId,
        long projectId,
        long edgeId,
        [FromQuery] bool hideArchived = true)
    {
        try
        {
            var edge = await _edgeBusiness.GetEdge(projectId, organizationId, edgeId, null, null, hideArchived);
            return Ok(edge);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while retrieving edge {edgeId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Get an Edge by Origin and Destination
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="originId">The origin ID of the edge to retrieve</param>
    /// <param name="destinationId">The destination ID of the edge to retrieve</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
    /// <returns>The edge associated with the given origin and destination</returns>
    [HttpGet("by-relationship", Name = "api_get_edge_by_relationship")]
    public async Task<ActionResult<EdgeResponseDto>> GetEdgeByRelationship(
        long organizationId,
        long projectId,
        [FromQuery] long originId,
        [FromQuery] long destinationId,
        [FromQuery] bool hideArchived = true)
    {
        try
        {
            var edge = await _edgeBusiness.GetEdge(projectId, organizationId, null, originId, destinationId,
                hideArchived);
            return Ok(edge);
        }
        catch (Exception exc)
        {
            var message =
                $"An error occurred while retrieving edge (origin: {originId}, destination: {destinationId}): {exc}";
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
            var createdEdge =
                await _edgeBusiness.CreateEdge(currentUserId, projectId, dataSourceId, organizationId, edge);
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
            var createdEdges =
                await _edgeBusiness.BulkCreateEdges(currentUserId, projectId, dataSourceId, organizationId, edges);
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
    ///     Update an Edge by ID
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="edgeId">The ID of the edge to update</param>
    /// <param name="dto">The edge request data transfer object containing updated edge details</param>
    /// <returns>The updated edge response DTO with its details</returns>
    [HttpPut("{edgeId:long}", Name = "api_update_edge_by_id")]
    public async Task<ActionResult<EdgeResponseDto>> UpdateEdgeById(
        long organizationId,
        long projectId,
        long edgeId,
        [FromBody] UpdateEdgeRequestDto dto)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var updatedEdge =
                await _edgeBusiness.UpdateEdge(currentUserId, projectId, organizationId, dto, edgeId, null, null);
            return Ok(updatedEdge);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while updating edge {edgeId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Update an Edge by Origin and Destination
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="originId">The origin ID of the edge to update</param>
    /// <param name="destinationId">The destination ID of the edge to update</param>
    /// <param name="dto">The edge request data transfer object containing updated edge details</param>
    /// <returns>The updated edge response DTO with its details</returns>
    [HttpPut("by-relationship", Name = "api_update_edge_by_relationship")]
    public async Task<ActionResult<EdgeResponseDto>> UpdateEdgeByRelationship(
        long organizationId,
        long projectId,
        [FromQuery] long originId,
        [FromQuery] long destinationId,
        [FromBody] UpdateEdgeRequestDto dto)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var updatedEdge =
                await _edgeBusiness.UpdateEdge(currentUserId, projectId, organizationId, dto, null, originId,
                    destinationId);
            return Ok(updatedEdge);
        }
        catch (Exception exc)
        {
            var message =
                $"An error occurred while updating edge (origin: {originId}, destination: {destinationId}): {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Delete an Edge by ID
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="edgeId">The ID of the edge to delete</param>
    /// <returns>A message stating the edge was successfully deleted.</returns>
    [HttpDelete("{edgeId:long}", Name = "api_delete_edge_by_id")]
    public async Task<IActionResult> DeleteEdgeById(
        long organizationId,
        long projectId,
        long edgeId)
    {
        try
        {
            await _edgeBusiness.DeleteEdge(projectId, organizationId, edgeId, null, null);
            return Ok(new { message = $"Deleted edge {edgeId}" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting edge {edgeId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Delete an Edge by Origin and Destination
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="originId">The origin ID of the edge to delete</param>
    /// <param name="destinationId">The destination ID of the edge to delete</param>
    /// <returns>A message stating the edge was successfully deleted.</returns>
    [HttpDelete("by-relationship", Name = "api_delete_edge_by_relationship")]
    public async Task<IActionResult> DeleteEdgeByRelationship(
        long organizationId,
        long projectId,
        [FromQuery] long originId,
        [FromQuery] long destinationId)
    {
        try
        {
            var edgeId = await _edgeBusiness.DeleteEdge(projectId, organizationId, null, originId, destinationId);
            return Ok(new { message = $"Deleted edge {edgeId}" });
        }
        catch (Exception exc)
        {
            var message =
                $"An error occurred while deleting edge (origin: {originId}, destination: {destinationId}): {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Archive or Unarchive an Edge by ID
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="edgeId">The ID of the edge to archive or unarchive</param>
    /// <param name="archive">True to archive the edge, false to unarchive it.</param>
    /// <returns>A message stating the edge was successfully archived or unarchived.</returns>
    [HttpPatch("{edgeId:long}", Name = "api_archive_edge_by_id")]
    public async Task<IActionResult> ArchiveEdgeById(
        long organizationId,
        long projectId,
        long edgeId,
        [FromQuery] bool archive)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            if (archive)
            {
                await _edgeBusiness.ArchiveEdge(currentUserId, organizationId, projectId, edgeId, null, null);
                return Ok(new { message = $"Archived edge {edgeId}" });
            }

            await _edgeBusiness.UnarchiveEdge(currentUserId, projectId, organizationId, edgeId, null, null);
            return Ok(new { message = $"Unarchived edge {edgeId}" });
        }
        catch (Exception exc)
        {
            var action = archive ? "archiving" : "unarchiving";
            var message = $"An error occurred while {action} edge {edgeId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Archive or Unarchive an Edge by Origin and Destination
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the edge belongs</param>
    /// <param name="originId">The origin ID of the edge to archive or unarchive</param>
    /// <param name="destinationId">The destination ID of the edge to archive or unarchive</param>
    /// <param name="archive">True to archive the edge, false to unarchive it.</param>
    /// <returns>A message stating the edge was successfully archived or unarchived.</returns>
    [HttpPatch("by-relationship", Name = "api_archive_edge_by_relationship")]
    public async Task<IActionResult> ArchiveEdgeByRelationship(
        long organizationId,
        long projectId,
        [FromQuery] long originId,
        [FromQuery] long destinationId,
        [FromQuery] bool archive)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            long edgeId;
            if (archive)
            {
                edgeId = await _edgeBusiness.ArchiveEdge(currentUserId, organizationId, projectId, null, originId,
                    destinationId);
                return Ok(new { message = $"Archived edge {edgeId}" });
            }

            edgeId = await _edgeBusiness.UnarchiveEdge(currentUserId, projectId, organizationId, null, originId,
                destinationId);
            return Ok(new { message = $"Unarchived edge {edgeId}" });
        }
        catch (Exception exc)
        {
            var action = archive ? "archiving" : "unarchiving";
            var message =
                $"An error occurred while {action} edge (origin: {originId}, destination: {destinationId}): {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}