using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/mappings")]
    public class RecordMappingController : ControllerBase
    {
        private readonly IRecordMappingBusiness _rMappingBusiness;

        public RecordMappingController(IRecordMappingBusiness rMappingBusiness)
        {
            _rMappingBusiness = rMappingBusiness;
        }

        [HttpGet("GetAllRecordMappings")]
        public async Task<IActionResult> GetAllRecordMappings(long projectId)
        {
            try
            {
                var rMappings = await _rMappingBusiness.GetAllRecordMappings(projectId);
                return Ok(rMappings);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing all record mappings: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpGet("GetRecordMapping/{mappingId}")]
        public async Task<IActionResult> GetRecordMapping(long mappingId)
        {
            try
            {
                var rMapping = await _rMappingBusiness.GetRecordMapping(mappingId);
                return Ok(rMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving record mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpPost("CreateRecordMapping")]
        public async Task<IActionResult> CreateRecordMapping(long projectId, [FromBody] RecordMappingRequestDto dto)
        {
            try
            {
                var rMapping = await _rMappingBusiness.CreateRecordMapping(projectId, dto);
                return Ok(rMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating record mapping: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpPut("UpdateRecordMapping/{mappingId}")]
        public async Task<IActionResult> UpdateRecordMapping(
            long projectId, 
            long mappingId, 
            [FromBody] RecordMappingRequestDto dto)
        {
            try
            {
                var rMapping = await _rMappingBusiness.UpdateRecordMapping(projectId, mappingId, dto);
                return Ok(rMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating record mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpDelete("DeleteRecordMapping/{mappingId}")]
        public async Task<IActionResult> DeleteRecordMapping(long mappingId)
        {
            try
            {
                await _rMappingBusiness.DeleteRecordMapping(mappingId);
                return NoContent();
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting record param {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}