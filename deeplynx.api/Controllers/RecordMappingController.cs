using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/parameters/records")]
    public class RecordParameterController : ControllerBase
    {
        private readonly IRecordParameterBusiness _rParamBusiness;

        public RecordParameterController(IRecordParameterBusiness rParamBusiness)
        {
            _rParamBusiness = rParamBusiness;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRecordParams(long projectId)
        {
            try
            {
                var rParams = await _rParamBusiness.GetAllRecordParameters(projectId);
                return Ok(rParams);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing all record parameters: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpGet("{recordParameterId}")]
        public async Task<IActionResult> GetRecordParam(long recordParameterId)
        {
            try
            {
                var rParam = await _rParamBusiness.GetRecordParameter(recordParameterId);
                return Ok(rParam);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing record parameter {recordParameterId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateRecordParam(long projectId, [FromBody] RecordParameterRequestDto dto)
        {
            try
            {
                var rParam = await _rParamBusiness.CreateRecordParameter(projectId, dto);
                return Ok(rParam);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating record parameter: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpPut("{recordParameterId}")]
        public async Task<IActionResult> UpdateRecordParam(
            long projectId, 
            long recordParameterId, 
            [FromBody] RecordParameterRequestDto dto)
        {
            try
            {
                var rParam = await _rParamBusiness.UpdateRecordParameter(projectId, recordParameterId, dto);
                return Ok(rParam);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating record parameter {recordParameterId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        [HttpDelete("{recordParameterId}")]
        public async Task<IActionResult> DeleteRecordParam(long recordParameterId)
        {
            try
            {
                await _rParamBusiness.DeleteRecordParameter(recordParameterId);
                return NoContent();
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting record param {recordParameterId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}