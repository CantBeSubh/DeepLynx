using System.ComponentModel.DataAnnotations;
using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("organizations/{organizationId}/projects/{projectId}/edges")]
    [Authorize]
    public class EdgeController : ControllerBase
    {
        private readonly IEdgeBusiness _edgeBusiness;
        private readonly ILogger<EdgeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeController"/> class
        /// </summary>
        /// <param name="edgeBusiness">The business logic interface for handling edge operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public EdgeController(IEdgeBusiness edgeBusiness, ILogger<EdgeController> logger)
        {
            _edgeBusiness = edgeBusiness;
            _logger = logger;
        }

        /// <summary>
        /// Get all edges
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project whose edges are to be retrieved</param>
        /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter edges</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
        /// <returns>A list of edges based on the applied filters.</returns>
        [HttpGet("GetAllEdges", Name = "api_get_all_edges")]
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
        /// Get edges by record
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project to which the record belongs</param>
        /// <param name="recordId">The ID of the datasource by which to filter edges</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
        /// <param name="isOrigin">Indicates whether to find where recordId is origin or not</param>
        /// <param name="page">Indicates the page number for pagination</param>
        /// <param name="pageSize">Indicates the page size for pagination</param>
        /// <returns>A list of edges based on the applied filters.</returns>
        [HttpGet("GetAllEdgesByRecord", Name = "api_get_edges_by_record")]
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
                var message = $"An error occurred while listing all edges: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get Graph Data
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project to which the record belongs</param>
        /// <param name="recordId">The ID of the datasource by which to filter edges</param>
        /// <param name="depth">The number of levels you want to search through</param>
        /// <returns>A list of edges based on the applied filters.</returns>
        [HttpGet("GetGraphDataForRecord", Name = "api_get_graph_data_for_record")]
        public async Task<ActionResult<GraphResponse>> GetGraphDataForRecord(
            long organizationId,
            long recordId,
            int depth)
        {
            try
            {
                var edges = await _edgeBusiness.GetGraphDataForRecord(recordId, UserContextStorage.UserId, depth);
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
        /// Get edge 
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="edgeId">The ID whereby to fetch the edge</param>
        /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
        /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
        /// <returns>The edge associated with the given id or origin/destination combo</returns>
        [HttpGet("GetEdge", Name = "api_get_an_edge")]
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
        /// Create edge 
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
        /// <param name="edge">The edge request data transfer object containing edge details</param>
        [HttpPost("CreateEdge", Name = "api_create_an_edge")]
        public async Task<ActionResult<EdgeResponseDto>> CreateEdge(
            long organizationId,
            long projectId, 
            [Required] long dataSourceId, 
            [FromBody] CreateEdgeRequestDto edge)
        {
            try
            {
                var createdEdge = await _edgeBusiness.CreateEdge(projectId, dataSourceId, edge);
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
        /// Create many edges 
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
        /// <param name="edges">List of the edge request data transfer objects containing edge details</param>
        [HttpPost("BulkCreateEdges", Name = "api_create_many_edges")]
        public async Task<ActionResult<List<EdgeResponseDto>>> BulkCreateEdges(
            long organizationId,
            long projectId,
            [Required] long dataSourceId,
            [FromBody] List<CreateEdgeRequestDto> edges)
        {
            try
            {
                var createdEdge = await _edgeBusiness.BulkCreateEdges(projectId, dataSourceId, edges);
                return Ok(createdEdge);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating edges: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update edge
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="dto">The edge request data transfer object containing updated edge details.</param>
        /// <param name="edgeId">The ID of the edge to update</param>
        /// <param name="originId">The origin ID of the edge to update if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
        /// <returns>The updated edge response DTO with its details</returns>
        [HttpPut("UpdateEdge", Name = "api_update_an_edge")]
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
                var updatedEdge = await _edgeBusiness.UpdateEdge(projectId, dto, edgeId, originId, destinationId);
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
        /// Delete edge
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="edgeId">The ID of the edge to delete</param>
        /// <param name="originId">The origin ID of the edge to delete if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
        /// <returns>A message stating the edge was successfully deleted.</returns>
        [HttpDelete("DeleteEdge", Name = "api_delete_an_edge")]
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
        /// Archive an edge
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="edgeId">The ID of the edge to archive</param>
        /// <param name="originId">The origin ID of the edge to archive if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge to archive if edgeID is not present.</param>
        /// <returns>A message stating the edge was successfully archived.</returns>
        [HttpDelete("ArchiveEdge", Name = "api_archive_an_edge")]
        public async Task<IActionResult> ArchiveEdge(
            long organizationId,
            long projectId,
            [FromQuery] long? edgeId,
            [FromQuery] long? originId,
            [FromQuery] long? destinationId)
        {
            try
            {
                edgeId = await _edgeBusiness.ArchiveEdge(projectId, edgeId, originId, destinationId);
                return Ok(new { message = $"Archived edge {edgeId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving edge: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Unarchive an edge
        /// </summary>
        /// <param name="organizationId">The ID of the organization to which the project belongs</param>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="edgeId">The ID of the edge to unarchive</param>
        /// <param name="originId">The origin ID of the edge to unarchive if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge to unarchive if edgeID is not present.</param>
        /// <returns>A message stating the edge was successfully unarchived.</returns>
        [HttpPut("UnarchiveEdge", Name = "api_unarchive_an_edge")]
        public async Task<IActionResult> UnarchiveEdge(
            long organizationId,
            long projectId,
            [FromQuery] long? edgeId,
            [FromQuery] long? originId,
            [FromQuery] long? destinationId)
        {
            try
            {
                edgeId = await _edgeBusiness.UnarchiveEdge(projectId, edgeId, originId, destinationId);
                return Ok(new { message = $"Unarchived edge {edgeId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving edge: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}