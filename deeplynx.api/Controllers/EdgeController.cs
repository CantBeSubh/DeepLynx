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
        /// Retrieves all edges for a specific project and (optionally) datasource
        /// </summary>
        /// <param name="projectId">The ID of the project whose edges are to be retrieved</param>
        /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter edges</param>
        /// <returns>A list of edges based on the applied filters.</returns>
        [HttpGet("GetAllEdges")]
        public async Task<IActionResult> GetAllEdges(long projectId, [FromQuery] long? dataSourceId = null)
        {
            try
            {
                var edges = await _edgeBusiness.GetAllEdges(projectId, dataSourceId);
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
        /// Retrieves a specific edge by its origin and destination IDs
        /// OR Retrieves an edge by its id
        /// </summary>
        /// <param name="edgeId">The id whereby to fetch the edge</param>
        /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
        /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
        /// <returns>The edge associated with the given id or origin/destination combo</returns>
        [HttpGet("GetEdge")]
        public async Task<IActionResult> GetEdge(
            [FromQuery] long? edgeId,
            [FromQuery] long? originId, 
            [FromQuery] long? destinationId)
        {
            try
            {
                var edge = await _edgeBusiness.GetEdge(edgeId, originId, destinationId);
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
        /// Retrieves a specific edge by its origin and destination IDs
        /// Asynchronously creates a new edge for a specified project.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs</param>
        /// <param name="dataSourceId">The ID of the data source to which the edge belongs</param>
        /// <param name="edge">The edge request data transfer object containing edge details</param>
        [HttpPost("CreateEdge")]
        public async Task<IActionResult> CreateEdge(long projectId, [Required] long dataSourceId, [FromBody] EdgeRequestDto edge)
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
        /// Updates an existing edge by its ID or origin/destination.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs.</param>
        /// <param name="edge">The edge request data transfer object containing updated edge details.</param>
        /// <param name="edgeId">The ID of the edge to update</param>
        /// <param name="originId">The origin ID of the edge to update if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
        /// <returns>The updated edge response DTO with its details</returns>
        /// <returns></returns>
        [HttpPut("UpdateEdge")]
        public async Task<IActionResult> UpdateEdge(
            long projectId,
            [FromBody] EdgeRequestDto edge,
            [FromQuery] long? edgeId,
            [FromQuery] long? originId, 
            [FromQuery] long? destinationId)
        {
            try
            {
                var updatedEdge = await _edgeBusiness.UpdateEdge(projectId, edge, edgeId, originId, destinationId);
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
        /// Deletes a specific edge by its ID or origin/destination.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge belongs.</param>
        /// <param name="edgeId">The ID of the edge to delete</param>
        /// <param name="originId">The origin ID of the edge to delete if edgeID is not present.</param>
        /// <param name="destinationId">The destination ID of the edge if edgeID is not present.</param>
        /// <param name="force">Indicates whether to force delete the edge if true.</param>
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
                await _edgeBusiness.DeleteEdge(projectId, edgeId, originId, destinationId, force);
                return Ok(new { message = $"Deleted edge {edgeId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting edge: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}