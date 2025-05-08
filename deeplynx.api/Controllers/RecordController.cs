using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/datasources/{dataSourceId}/records")]
    public class RecordController : ControllerBase
    {
        private readonly IRecordBusiness _recordBusiness;

        public RecordController(IRecordBusiness recordBusiness)
        {
            _recordBusiness = recordBusiness;
        }

        [HttpGet("GetAllRecords")]
        public async Task<IActionResult> GetAllRecords(long projectId, long dataSourceId)
        {
            try
            {
                var records = await _recordBusiness.GetAllRecords(projectId, dataSourceId);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing roles: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpGet("GetRecord/{recordId}")]
        public async Task<IActionResult> GetRecord(long projectId, long dataSourceId, long recordId)
        {
            try
            {
                var record = await _recordBusiness.GetRecord(projectId, dataSourceId, recordId);
                return Ok(record);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpPost("CreateRecord")]
        public async Task<IActionResult> CreateRecord(long projectId, long dataSourceId,
            [FromBody] RecordRequestDto dto)
        {
            try
            {
                var record = await _recordBusiness.CreateRecord(projectId, dataSourceId, dto);
                return Ok(record);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating record: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpPut("UpdateRecord/{recordId}")]
        public async Task<IActionResult> UpdateRecord(long projectId, long dataSourceId, long recordId,
            [FromBody] RecordRequestDto dto)
        {
            try
            {
                var updated = await _recordBusiness.UpdateRecord(projectId, dataSourceId, recordId, dto);
                return Ok(updated);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpDelete("DeleteRecord/{recordId}")]
        public async Task<IActionResult> DeleteRecord(long projectId, long dataSourceId, long recordId)
        {
            try
            {
                await _recordBusiness.DeleteRecord(projectId, dataSourceId, recordId);
                return Ok(new { message = $"Deleted record {recordId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
            
    }
}

