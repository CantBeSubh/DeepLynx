using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("projects/{projectId}/relationships")]
    public class RelationshipController : ControllerBase
    {
        private readonly IRelationshipBusiness _business;

        public RelationshipController(IRelationshipBusiness business)
        {
            _business = business;
        }
        
    /// <summary>
    /// Get all Relationships from the database
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
        [HttpGet("GetAllRelationships")]
        public async Task<IActionResult> GetAll(long projectId)
        {
            try
            {
                var relationships = await _business.GetAllRelationships(projectId);
                return Ok(relationships);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching all relationships.: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get one Relationship from DB
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="relationshipId"></param>
        /// <returns></returns>
        [HttpGet("GetAllRelationships/{relationshipId}")]
        public async Task<IActionResult> Get(long projectId, long relationshipId)
        {
            try
            {
                return Ok(await _business.GetRelationship(projectId, relationshipId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            } catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching the relationship {relationshipId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Create one Relationship
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("CreateRelationship")]
        public async Task<IActionResult> CreateRelationship(long projectId, [FromBody] RelationshipRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid input", details = ModelState });
            }

            try
            {
                var created = await _business.CreateRelationship(projectId, dto);
                // Re-use the clean, Include-ready method
                var full = await _business.GetRelationship(projectId, created.Id);

                return Ok(full);
            }
            catch (Exception exc)
            {
                var message = $"Unexpected error while creating relationship.: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Update Relationship
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="relationshipId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("UpdateRelationship/{relationshipId}")]
        public async Task<IActionResult> UpdateRelationship(long projectId, long relationshipId,
            [FromBody] RelationshipRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid input", details = ModelState });
            }
            try
            {
                var result = await _business.UpdateRelationship(projectId, relationshipId, dto);
                return Ok(result);
            }
            catch (Exception exc)
            {
                var message = $"Unexpected error while updating relationship {relationshipId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Delete Relationship
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="relationshipId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteRelationship/{relationshipId}")]
        public async Task<IActionResult> DeleteRelationship(long projectId, long relationshipId)
        {
            try
            {
                var success = await _business.DeleteRelationship(projectId, relationshipId);
                return Ok(new { message = $"Relationship successfully deleted.{relationshipId}" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception exc)
            {
                var message = $"Unexpected error while deleting relationship {relationshipId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}