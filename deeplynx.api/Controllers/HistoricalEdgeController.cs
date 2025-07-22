using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/records/historical")]
    public class HistoricalEdgeController : ControllerBase
    {
        private readonly IHistoricalEdgeBusiness _historicalEdgeBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalEdgeController"/> class
        /// </summary>
        /// <param name="historicalEdgeBusiness">The business logic interface for handling record operations.</param>
        public HistoricalEdgeController(IHistoricalEdgeBusiness historicalEdgeBusiness)
        {
            _historicalEdgeBusiness = historicalEdgeBusiness;
        }
        
        /// <summary>
        /// Get all edges at a point in time (defaults to current)
        /// </summary>
        /// <param name="projectId">Project ID which edges are associated with</param>
        /// <param name="dataSourceId">(Optional) Datasource ID which edges are associated with</param>
        /// <param name="pointInTime">(Optional) Find the most current edges that existed before this point in time</param>
        /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived edges from the result (Default true)</param>
        /// <param name="current">(Optional) Find only the most current edges. Overrides point in time (Default true)</param>
        /// <returns>List of edge response DTOs</returns>
        [HttpGet("GetAllHistoricalEdges")]
        public async Task<ActionResult<IEnumerable<HistoricalEdgeResponseDto>>> GetAllHistoricalEdges(
            long projectId,
            [FromQuery] long? dataSourceId,
            [FromQuery] DateTime? pointInTime,
            [FromQuery] bool hideArchived = true,
            [FromQuery] bool current = true)
        {
            try
            {
                var edges = await _historicalEdgeBusiness
                    .GetAllHistoricalEdges(projectId, dataSourceId, pointInTime, hideArchived, current);
                return Ok(edges);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing historical edges: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get an edge at a point in time (defaults to current)
        /// </summary>
        /// <param name="edgeId">The ID whereby to fetch the edge</param>
        /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
        /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
        /// <param name="pointInTime">(Optional) Find the most current edges that existed before this point in time</param>
        /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived edges from the result (Default true)</param>
        /// <param name="current">(Optional) Find only the most current edges. Overrides point in time (Default true)</param>
        /// <returns>Edge response DTO</returns>
        [HttpGet("GetHistoricalEdge")]
        public async Task<ActionResult<HistoricalEdgeResponseDto>> GetHistoricalEdge(
            [FromQuery] long? edgeId,
            [FromQuery] long? originId, 
            [FromQuery] long? destinationId,
            [FromQuery] DateTime? pointInTime, 
            [FromQuery] bool hideArchived = true,
            [FromQuery] bool current = true)
        {
            try
            {
                var edge = await _historicalEdgeBusiness
                    .GetHistoricalEdge(edgeId, originId, destinationId, pointInTime, hideArchived, current);
                return Ok(edge);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving historical edge: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get history for an edge
        /// </summary>
        /// <param name="edgeId">The ID whereby to fetch the edge</param>
        /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
        /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
        /// <returns>A list of previous edge versions</returns>
        [HttpGet("GetEdgeHistory")]
        public async Task<ActionResult<HistoricalEdgeResponseDto>> GetEdgeHistory(
            [FromQuery] long? edgeId,
            [FromQuery] long? originId, 
            [FromQuery] long? destinationId)
        {
            try
            {
                var edge = await _historicalEdgeBusiness
                    .GetHistoryForEdge(edgeId, originId, destinationId);
                return Ok(edge);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving history for edge: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}

