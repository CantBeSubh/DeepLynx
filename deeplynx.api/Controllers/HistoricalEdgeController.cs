using deeplynx.helpers;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("/projects/{projectId}/records/historical")]
    [NexusAuthorize]
    public class HistoricalEdgeController : ControllerBase
    {
        private readonly IHistoricalEdgeBusiness _historicalEdgeBusiness;
        private readonly ILogger<HistoricalEdgeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalEdgeController"/> class
        /// </summary>
        /// <param name="historicalEdgeBusiness">The business logic interface for handling record operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public HistoricalEdgeController(IHistoricalEdgeBusiness historicalEdgeBusiness, ILogger<HistoricalEdgeController> logger)
        {
            _historicalEdgeBusiness = historicalEdgeBusiness;
            _logger = logger;
        }
        
        /// <summary>
        /// Get all edges at a point in time
        /// </summary>
        /// <param name="projectId">Project ID which edges are associated with</param>
        /// <param name="dataSourceId">(Optional) Datasource ID which edges are associated with</param>
        /// <param name="pointInTime">(Optional) Find the most current edges that existed before this point in time</param>
        /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived edges from the result (Default true)</param>
        /// <param name="current">(Optional) Find only the most current edges. Overrides point in time (Default true)</param>
        /// <returns>List of edge response DTOs</returns>
        [HttpGet("GetAllHistoricalEdges", Name = "api_get_all_historical_edges")]
        public async Task<ActionResult<IEnumerable<HistoricalEdgeResponseDto>>> GetAllHistoricalEdges(
            long projectId,
            [FromQuery] long? dataSourceId,
            [FromQuery] DateTime? pointInTime,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var edges = await _historicalEdgeBusiness
                    .GetAllHistoricalEdges(projectId, dataSourceId, pointInTime, hideArchived);
                return Ok(edges);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing historical edges: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get an edge at a point in time
        /// </summary>
        /// <param name="edgeId">The ID whereby to fetch the edge</param>
        /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
        /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
        /// <param name="pointInTime">(Optional) Find the most current edges that existed before this point in time</param>
        /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived edges from the result (Default true)</param>
        /// <param name="current">(Optional) Find only the most current edges. Overrides point in time (Default true)</param>
        /// <returns>Edge response DTO</returns>
        [HttpGet("GetHistoricalEdge", Name = "api_get_a_historical_edge")]
        public async Task<ActionResult<HistoricalEdgeResponseDto>> GetHistoricalEdge(
            [FromQuery] long? edgeId,
            [FromQuery] long? originId, 
            [FromQuery] long? destinationId,
            [FromQuery] DateTime? pointInTime, 
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var edge = await _historicalEdgeBusiness
                    .GetHistoricalEdge(edgeId, originId, destinationId, pointInTime, hideArchived);
                return Ok(edge);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving historical edge: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get edge history
        /// </summary>
        /// <param name="edgeId">The ID whereby to fetch the edge</param>
        /// <param name="originId">the origin ID by which to fetch the edge if no ID</param>
        /// <param name="destinationId">the destination ID by which to fetch the edge if no ID</param>
        /// <returns>A list of previous edge versions</returns>
        [HttpGet("GetEdgeHistory", Name = "api_get_an_edge_history")]
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
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}

