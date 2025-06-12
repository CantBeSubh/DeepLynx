using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/records")]
    public class RecordController : ControllerBase
    {
        private readonly IRecordBusiness _recordBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordController"/> class
        /// </summary>
        /// <param name="recordBusiness">The business logic interface for handling record operations.</param>
        public RecordController(IRecordBusiness recordBusiness)
        {
            _recordBusiness = recordBusiness;
        }
        
        /// <summary>
        /// Get All Records
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="dataSourceId"></param>
        /// <returns></returns>
        [HttpGet("GetAllRecords")]
        public async Task<IActionResult> GetAllRecords(long projectId, [FromQuery] long? dataSourceId)
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
        
        /// <summary>
        /// Get one Record from DB
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        [HttpGet("GetRecord/{recordId}")]
        public async Task<IActionResult> GetRecord(long projectId, long recordId)
        {
            try
            {
                var record = await _recordBusiness.GetRecord(projectId, recordId);
                return Ok(record);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Create a Record
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="dataSourceId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("CreateRecord")]
        public async Task<IActionResult> CreateRecord(
            long projectId, 
            [FromQuery] long dataSourceId,
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
        
        /// <summary>
        /// Update Record
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="recordId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("UpdateRecord/{recordId}")]
        public async Task<IActionResult> UpdateRecord(
            long projectId,
            long recordId,
            [FromBody] RecordRequestDto dto)
        {
            try
            {
                var updated = await _recordBusiness.UpdateRecord(projectId, recordId, dto);
                return Ok(updated);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="recordId"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        [HttpDelete("DeleteRecord/{recordId}")]
        public async Task<IActionResult> DeleteRecord(long projectId, long recordId, [FromQuery] bool force = false)
        {
            try
            {
                await _recordBusiness.DeleteRecord(projectId, recordId, force);
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

