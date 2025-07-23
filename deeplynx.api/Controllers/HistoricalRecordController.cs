using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/records/historical")]
    public class HistoricalRecordController : ControllerBase
    {
        private readonly IHistoricalRecordBusiness _historicalRecordBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalRecordController"/> class
        /// </summary>
        /// <param name="historicalRecordBusiness">The business logic interface for handling record operations.</param>
        public HistoricalRecordController(IHistoricalRecordBusiness historicalRecordBusiness)
        {
            _historicalRecordBusiness = historicalRecordBusiness;
        }
        
        /// <summary>
        /// Get all records at a point in time
        /// </summary>
        /// <param name="projectId">Project ID which records are associated with</param>
        /// <param name="dataSourceId">(Optional) Datasource ID which records are associated with</param>
        /// <param name="pointInTime">(Optional) Find the most current records that existed before this point in time</param>
        /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived records from the result (Default true)</param>
        /// <param name="current">(Optional) Find only the most current records. Overrides point in time (Default true)</param>
        /// <returns>List of record response DTOs</returns>
        [HttpGet("GetAllHistoricalRecords")]
        public async Task<ActionResult<IEnumerable<HistoricalRecordResponseDto>>> GetAllHistoricalRecords(
            long projectId,
            [FromQuery] long? dataSourceId,
            [FromQuery] DateTime? pointInTime,
            [FromQuery] bool hideArchived = true,
            [FromQuery] bool current = true)
        {
            try
            {
                var records = await _historicalRecordBusiness
                    .GetAllHistoricalRecords(projectId, dataSourceId, pointInTime, hideArchived, current);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing historical records: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get a record at a point in time
        /// </summary>
        /// <param name="recordId">ID of record to search on</param>
        /// <param name="pointInTime">(Optional) Find the most current records that existed before this point in time</param>
        /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived records from the result (Default true)</param>
        /// <param name="current">(Optional) Find only the most current records. Overrides point in time (Default true)</param>
        /// <returns>Record response DTO</returns>
        [HttpGet("GetHistoricalRecord/{recordId}")]
        public async Task<ActionResult<HistoricalRecordResponseDto>> GetHistoricalRecord(
            long recordId,
            [FromQuery] DateTime? pointInTime, 
            [FromQuery] bool hideArchived = true,
            [FromQuery] bool current = true)
        {
            try
            {
                var record = await _historicalRecordBusiness
                    .GetHistoricalRecord(recordId, pointInTime, hideArchived, current);
                return Ok(record);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving historical record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get record history
        /// </summary>
        /// <param name="recordId">ID of record to search on </param>
        /// <returns>A list of previous record versions</returns>
        [HttpGet("GetRecordHistory/{recordId}")]
        public async Task<ActionResult<HistoricalRecordResponseDto>> GetRecordHistory(long recordId)
        {
            try
            {
                var record = await _historicalRecordBusiness
                    .GetHistoryForRecord(recordId);
                return Ok(record);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving history for record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}

