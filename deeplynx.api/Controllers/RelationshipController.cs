using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/relationships")]
    public class RelationshipController : ControllerBase
    {
        private readonly IRelationshipBusiness _business;

        public RelationshipController(IRelationshipBusiness business)
        {
            _business = business;
        }

    /// <summary>
    /// Get all relationships 
    /// </summary>
    /// <param name="projectId">ID for project which relationship is associated with</param>
    /// <returns>List of relationship response DTOs</returns>
        [HttpGet("GetAllRelationships")]
        public async Task<IActionResult> GetAllRelationships(long projectId)
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
        /// Get a relationship
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="relationshipId">Id of relationship</param>
        /// <returns>Relationship response DTO</returns>
        [HttpGet("GetRelationship/{relationshipId}")]
        public async Task<IActionResult> GetRelationship(long projectId, long relationshipId)
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
        /// Create a relationship 
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="dto">Relationship request DTO</param>
        /// <returns>Relationship response DTO</returns>
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
        /// Update a relationship 
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="relationshipId">Relationship ID</param>
        /// <param name="dto">Relationship request DTO</param>
        /// <returns>Relationship response DTO</returns>
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
        /// Delete a relationship
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="relationshipId">Relationship ID</param>
        /// <returns>Relationship was successfully deleted.</returns>
        [HttpDelete("DeleteRelationship/{relationshipId}")]
        public async Task<IActionResult> DeleteRelationship(long projectId, long relationshipId)
        {
            try
            {
                await _business.DeleteRelationship(projectId, relationshipId);
                return Ok(new { message = $"Relationship with ID {relationshipId} was successfully deleted." });
            }
            catch (Exception exc)
            {
                var message = $"Unexpected error while deleting relationship {relationshipId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive a relationship 
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="relationshipId">Relationship ID</param>
        /// <returns>Relationship was successfully archived.</returns>
        [HttpDelete("ArchiveRelationship/{relationshipId}")]
        public async Task<IActionResult> ArchiveRelationship(long projectId, long relationshipId)
        {
            try
            {
                await _business.ArchiveRelationship(projectId, relationshipId);
                return Ok(new { message = $"Relationship with ID {relationshipId} was successfully archived." });
            }
            catch (Exception exc)
            {
                var message = $"Unexpected error while archiving relationship {relationshipId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}