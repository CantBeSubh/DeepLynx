using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("projects/{projectId}/relationships")]
    [Authorize]
    public class RelationshipController : ControllerBase
    {
        private readonly IRelationshipBusiness _business;
        private readonly ILogger<RelationshipController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipController"/> class
        /// </summary>
        /// <param name="business">The business logic interface for handling relationship operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public RelationshipController(IRelationshipBusiness business, ILogger<RelationshipController> logger)
        {
            _business = business;
            _logger = logger;
        }

        /// <summary>
        /// Get all relationships
        /// </summary>
        /// <param name="projectId">ID for project which relationship is associated with</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived relationships from the result (Default true)</param>
        /// <returns>List of relationship response DTOs</returns>
        [HttpGet("GetAllRelationships", Name = "api_get_all_relationships")]

        public async Task<ActionResult<IEnumerable<RelationshipResponseDto>>> GetAllRelationships(
        long projectId,
        [FromQuery] bool hideArchived = true)
        {
            try
            {
                var relationships = await _business.GetAllRelationships(projectId,  hideArchived);
                return Ok(relationships);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching all relationships.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get a relationship
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="relationshipId">Id of relationship</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived relationships from the result (Default true)</param>
        /// <returns>Relationship response DTO</returns>
        [HttpGet("GetRelationship/{relationshipId}", Name = "api_get_a_relationship")]
        public async Task<ActionResult<RelationshipResponseDto>> GetRelationship(
            long projectId, 
            long relationshipId,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                return Ok(await _business.GetRelationship(projectId, relationshipId, hideArchived));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            } catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching the relationship {relationshipId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create a relationship 
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="dto">Relationship request DTO</param>
        /// <returns>Relationship response DTO</returns>
        [HttpPost("CreateRelationship", Name = "api_create_a_relationship")]
        public async Task<ActionResult<RelationshipResponseDto>> CreateRelationship(long projectId, [FromBody] CreateRelationshipRequestDto dto)
        {
            try
            {
                var created = await _business.CreateRelationship(projectId, dto);
                return Ok(created);
            }
            catch (Exception exc)
            {
                var message = $"Unexpected error while creating relationship.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Create many relationships 
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="relationships">Relationship request DTOs</param>
        /// <returns>Relationship response DTO</returns>
        [HttpPost("BulkCreateRelationships", Name = "api_create_many_relationships")]
        public async Task<ActionResult<List<RelationshipResponseDto>>> BulkCreateRelationships(
            long projectId, 
            [FromBody] List<CreateRelationshipRequestDto> relationships)
        {
            try
            {
                var created = await _business.BulkCreateRelationships(projectId, relationships);
                return Ok(created);
            }
            catch (Exception exc)
            {
                var message = $"Unexpected error while creating relationships: {exc}";
                _logger.LogError(message);
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
        [HttpPut("UpdateRelationship/{relationshipId}", Name = "api_update_a_relationship")]
        public async Task<ActionResult<RelationshipResponseDto>> UpdateRelationship(long projectId, long relationshipId,
            [FromBody] UpdateRelationshipRequestDto dto)
        {
            try
            {
                var result = await _business.UpdateRelationship(projectId, relationshipId, dto);
                return Ok(result);
            }
            catch (Exception exc)
            {
                var message = $"Unexpected error while updating relationship {relationshipId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete a relationship
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="relationshipId">Relationship ID</param>
        /// <returns>Relationship was successfully deleted.</returns>
        [HttpDelete("DeleteRelationship/{relationshipId}", Name = "api_delete_a_relationship")]
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
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive a relationship 
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="relationshipId">Relationship ID</param>
        /// <returns>A message stating the relationship was successfully archived.</returns>
        [HttpDelete("ArchiveRelationship/{relationshipId}", Name = "api_archive_a_relationship")]
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
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unarchive a relationship 
        /// </summary>
        /// <param name="projectId">ID for project relationship is associated with</param>
        /// <param name="relationshipId">Relationship ID</param>
        /// <returns>A message stating the relationship was successfully unarchived.</returns>
        [HttpPut("UnarchiveRelationship/{relationshipId}", Name = "api_unarchive_a_relationship")]
        public async Task<IActionResult> UnarchiveRelationship(long projectId, long relationshipId)
        {
            try
            {
                await _business.UnarchiveRelationship(projectId, relationshipId);
                return Ok(new { message = $"Relationship with ID {relationshipId} was successfully unarchived." });
            }
            catch (Exception exc)
            {
                var message = $"Unexpected error while unarchiving relationship {relationshipId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}