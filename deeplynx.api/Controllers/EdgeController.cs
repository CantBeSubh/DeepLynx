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

        public EdgeController(IEdgeBusiness edgeBusiness)
        {
            _edgeBusiness = edgeBusiness;
        }

        [HttpGet]
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

        [HttpGet("edge")]
        public async Task<IActionResult> GetEdge(
            [FromQuery, Required] long originId, 
            [FromQuery, Required] long destinationId)
        {
            try
            {
                var edge = await _edgeBusiness.GetEdge(originId, destinationId);
                return Ok(edge);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving edge with origin {originId} and destination {destinationId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpPost]
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

        [HttpPut("edge")]
        public async Task<IActionResult> UpdateEdge(
            [FromQuery, Required] long originId, 
            [FromQuery, Required] long destinationId, 
            [FromBody] EdgeRequestDto edge)
        {
            try
            {
                var updatedEdge = await _edgeBusiness.UpdateEdge(originId, destinationId, edge);
                return Ok(updatedEdge);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating edge with origin {originId} and destination {destinationId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpDelete("edge")]
        public async Task<IActionResult> DeleteEdge(
            [FromQuery, Required] long originId, 
            [FromQuery, Required] long destinationId)
        {
            try
            {
                await _edgeBusiness.DeleteEdge(originId, destinationId);
                return NoContent();
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting edge with origin {originId} and destination {destinationId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}