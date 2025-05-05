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

        [HttpGet]
        public async Task<IActionResult> GetAll(long projectId) =>
            Ok(await _business.GetAllRelationships(projectId));

        [HttpGet("{relationshipId}")]
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

        [HttpPost]
        public async Task<IActionResult> Create(long projectId, [FromBody] RelationshipRequestDto dto)
        {
            var result = await _business.CreateRelationship(projectId, dto);
            return Ok(result);
        }

        [HttpPut("{relationshipId}")]
        public async Task<IActionResult> Update(long projectId, long relationshipId,
            [FromBody] RelationshipRequestDto dto)
        {
            try
            {
                var result = await _business.UpdateRelationship(projectId, relationshipId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpDelete("{relationshipId}")]
        public async Task<IActionResult> Delete(long projectId, long relationshipId)
        {
            try
            {
                var success = await _business.DeleteRelationship(projectId, relationshipId);
                return Ok(new { message = "Relationship successfully deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected error while deleting relationship." });
            }
        }
    }
}