using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/mappings")]
    public class EdgeMappingController : ControllerBase
    {
        private readonly IEdgeMappingBusiness _eMappingBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeMappingController"/> class
        /// </summary>
        /// <param name="eMappingBusiness">The business logic interface for handling edge mapping operations.</param>
        public EdgeMappingController(IEdgeMappingBusiness eMappingBusiness)
        {
            _eMappingBusiness = eMappingBusiness;
        }

        /// <summary>
        /// Get all edge mappings
        /// </summary>
        /// <param name="projectId">The ID of the project whose mappings are to be retrieved</param>
        /// <param name="classId">(Optional) The ID of the origin or destination class by which to filter mappings</param>
        /// <param name="relationshipId">(Optional) The ID of the relationship by which to filter mappings</param>
        /// <returns>A list of edge mappings based on the applied filters.</returns>
        [HttpGet("GetAllEdgeMappings")]
        public async Task<IActionResult> GetAllEdgeMappings(
            long projectId,
            [FromQuery] long? classId = null,
            [FromQuery] long? relationshipId = null)
        {
            try
            {
                var rMappings = await _eMappingBusiness
                    .GetAllEdgeMappings(projectId, classId, relationshipId);
                return Ok(rMappings);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing all edge mappings: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get edge mapping
        /// </summary>
        /// <param name="mappingId">The ID whereby to fetch the edge mapping</param>
        /// <param name="projectId">The ID of the project to which the edge mapping belongs</param>
        /// <returns>The edge mapping associated with the given ID</returns>
        [HttpGet("GetEdgeMapping/{mappingId}")]
        public async Task<IActionResult> GetEdgeMapping(long projectId, long mappingId)
        {
            try
            {
                var rMapping = await _eMappingBusiness.GetEdgeMapping(projectId, mappingId);
                return Ok(rMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving edge mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create edge mapping
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge mapping belongs</param>
        /// <param name="dto">The edge mapping request data transfer object containing mapping details</param>
        /// <returns>The created edge mapping</returns>
        [HttpPost("CreateEdgeMapping")]
        public async Task<IActionResult> CreateEdgeMapping(
            long projectId,
            [FromBody] EdgeMappingRequestDto dto)
        {
            try
            {
                var rMapping = await _eMappingBusiness.CreateEdgeMapping(projectId, dto);
                return Ok(rMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating edge mapping: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update edge mapping
        /// </summary>
        /// <param name="projectId">The ID of the project to which the edge mapping belongs.</param>
        /// <param name="mappingId">The ID of the edge mapping to update.</param>
        /// <param name="dto">The edge mapping request data transfer object containing updated details.</param>
        /// <returns>The updated mapping response DTO with its details.</returns>
        [HttpPut("UpdateEdgeMapping/{mappingId}")]
        public async Task<IActionResult> UpdateEdgeMapping(
            long projectId,
            long mappingId,
            [FromBody] EdgeMappingRequestDto dto)
        {
            try
            {
                var rMapping = await _eMappingBusiness.UpdateEdgeMapping(projectId, mappingId, dto);
                return Ok(rMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating record mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete edge mapping
        /// </summary>
        /// <param name="mappingId">The ID of the edge mapping to delete.</param>
        /// <param name="projectId">The ID of the project to which the mapping belongs.</param>
        /// <returns>A message stating the edge mapping was successfully deleted.</returns>
        [HttpDelete("DeleteEdgeMapping/{mappingId}")]
        public async Task<IActionResult> DeleteEdgeMapping(long projectId, long mappingId)
        {
            try
            {
                await _eMappingBusiness.DeleteEdgeMapping(projectId, mappingId);
                return Ok(new { message = $"Deleted edge mapping {mappingId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting edge mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive a specific edge mapping
        /// </summary>
        /// <param name="mappingId">The ID of the edge mapping to archive.</param>
        /// <param name="projectId">The ID of the project to which the mapping belongs.</param>
        /// <returns>A message stating the edge mapping was successfully archived.</returns>
        [HttpDelete("ArchiveEdgeMapping/{mappingId}")]
        public async Task<IActionResult> ArchiveEdgeMapping(long projectId, long mappingId)
        {
            try
            {
                await _eMappingBusiness.ArchiveEdgeMapping(projectId, mappingId);
                return Ok(new { message = $"Archived edge mapping {mappingId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving edge mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}