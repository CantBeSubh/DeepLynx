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

        public EdgeMappingController(IEdgeMappingBusiness eMappingBusiness)
        {
            _eMappingBusiness = eMappingBusiness;
        }
        
        [HttpGet("GetAllEdgeMappings")]
        public async Task<IActionResult> GetAllEdgeMappings(long projectId)
        {
            try
            {
                var eMappings = await _eMappingBusiness
                    .GetAllEdgeMappings(projectId);
                return Ok(eMappings);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing all edge mappings: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpGet("GetEdgeMapping/{mappingId}")]
        public async Task<IActionResult> GetEdgeMapping(long mappingId)
        {
            try
            {
                var eMapping = await _eMappingBusiness.GetEdgeMapping(mappingId);
                return Ok(eMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving edge mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpPost("CreateEdgeMapping")]
        public async Task<IActionResult> CreateEdgeMapping(
            long projectId, 
            [FromBody] EdgeMappingRequestDto dto)
        {
            try
            {
                var eMapping = await _eMappingBusiness.CreateEdgeMapping(projectId, dto);
                return Ok(eMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating edge mapping: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpPut("UpdateEdgeMapping/{mappingId}")]
        public async Task<IActionResult> UpdateEdgeMapping(
            long projectId, 
            long mappingId, 
            [FromBody] EdgeMappingRequestDto dto)
        {
            try
            {
                var eMapping = await _eMappingBusiness
                    .UpdateEdgeMapping(projectId, mappingId, dto);
                return Ok(eMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating edge mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpDelete("DeleteEdgeMapping/{mappingId}")]
        public async Task<IActionResult> DeleteEdgeMapping(long mappingId)
        {
            try
            {
                await _eMappingBusiness.DeleteEdgeMapping(mappingId);
                return Ok(new { message = $"Deleted edge mapping {mappingId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting edge param {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}