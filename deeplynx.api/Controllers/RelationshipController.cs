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

        [HttpGet("GetAllRelationships")]
        public async Task<IActionResult> GetAll(long projectId)
        {
            try
            {
                var relationships = await _business.GetAllRelationships(projectId);
                return Ok(relationships);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new
                    {
                        error = "AN unexpected error occurred while fetching all relationships.", details = ex.Message
                    });
            }
        }


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
            }
        }

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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected error while creating relationship." , details= ex.Message });
            }
        }

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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected error while updating relationship." , details= ex.Message });
            }
        }

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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected error while deleting relationship." ,details= ex.Message });
            }
        }
    }
}