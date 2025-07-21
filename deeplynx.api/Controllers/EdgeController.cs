using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/edges")]
    
    public class EdgeController : ControllerBase
    {
        private readonly IEdgeBusiness _edgeBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeController"/> class
        /// </summary>
        /// <param name="edgeBusiness">The business logic interface for handling edge operations.</param>
        public EdgeController(IEdgeBusiness edgeBusiness)
        {
            _edgeBusiness = edgeBusiness;
        }

        /// <summary>
        /// Get all edges
        /// </summary>
        /// <param name="projectId">The ID of the project whose edges are to be retrieved</param>
        /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter edges</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
        /// <returns>A list of edges based on the applied filters.</returns>
        [HttpGet("GetAllEdges")]
        public async Task<ActionResult<IEnumerable<EdgeResponseDto>>> GetAllEdges(
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get edge 
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="edgeId">The id whereby to fetch the edge</param>
        /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
        /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived edges from the result (Default true)</param>
        /// <returns>The edge associated with the given id or origin/destination combo</returns>
        [HttpGet("GetEdge")]
        public async Task<ActionResult<EdgeResponseDto>> GetEdge(
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create edge 
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
        /// <param name="edge">The edge request data transfer object containing edge details</param>
        [HttpPost("CreateEdge")]
        public async Task<ActionResult<EdgeResponseDto>> CreateEdge(long projectId, [Required] long dataSourceId, [FromBody] EdgeRequestDto edge)
        {
            try
            {
                var createdEdge = await _edgeBusiness.CreateEdge(projectId, dataSourceId, edge);
                return Ok(createdEdge);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating edge: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Create many edges 
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
        /// <param name="edge">The edge request data transfer object containing edge details</param>
        [HttpPost("BulkCreateEdges")]
        public async Task<ActionResult<BulkEdgeResponseDto>> BulkCreateEdges(long projectId, [Required] long dataSourceId, [FromBody] BulkEdgeRequestDto edge)
        {
            try
            {
                var createdEdge = await _edgeBusiness.BulkCreateEdges(projectId, dataSourceId, edge);
                return Ok(createdEdge);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating edges: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update edge
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs.</param>
        /// <param name="dto">The edge request data transfer object containing updated edge details.</param>
        /// <param name="edgeId">The ID of the edge to update</param>
        /// <param name="originId">The origin ID of the edge to update if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
        /// <returns>The updated edge response DTO with its details</returns>
        [HttpPut("UpdateEdge")]
        public async Task<ActionResult<EdgeResponseDto>> UpdateEdge(
            long projectId,
            [FromBody] EdgeRequestDto dto,
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete edge
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs.</param>
        /// <param name="edgeId">The ID of the edge to delete</param>
        /// <param name="originId">The origin ID of the edge to delete if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
        /// <returns>A message stating the edge was successfully deleted.</returns>
        [HttpDelete("DeleteEdge")]
        public async Task<IActionResult> DeleteEdge(
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive an edge
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs.</param>
        /// <param name="edgeId">The ID of the edge to archive</param>
        /// <param name="originId">The origin ID of the edge to archive if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge to archive if edgeID is not present.</param>
        /// <returns>A message stating the edge was successfully archived.</returns>
        [HttpDelete("ArchiveEdge")]
        public async Task<IActionResult> ArchiveEdge(
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unarchive an edge
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs.</param>
        /// <param name="edgeId">The ID of the edge to unarchive</param>
        /// <param name="originId">The origin ID of the edge to unarchive if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge to unarchive if edgeID is not present.</param>
        /// <returns>A message stating the edge was successfully unarchived.</returns>
        [HttpPut("UnarchiveEdge")]
        public async Task<IActionResult> UnarchiveEdge(
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}