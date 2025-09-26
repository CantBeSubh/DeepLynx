using deeplynx.helpers;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/records/historical")]
    [NexusAuthorize]
    public class HistoricalRecordController : ControllerBase
    {
        private readonly IHistoricalRecordBusiness _historicalRecordBusiness;
        private readonly ILogger<HistoricalRecordController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalRecordController"/> class
        /// </summary>
        /// <param name="historicalRecordBusiness">The business logic interface for handling record operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public HistoricalRecordController(IHistoricalRecordBusiness historicalRecordBusiness, ILogger<HistoricalRecordController> logger)
        {
            _historicalRecordBusiness = historicalRecordBusiness;
            _logger = logger;
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
        [HttpGet("GetAllHistoricalRecords", Name = "api_get_all_historical_records")]
        public async Task<ActionResult<IEnumerable<HistoricalRecordResponseDto>>> GetAllHistoricalRecords(
            long projectId,
            [FromQuery] long? dataSourceId,
            [FromQuery] DateTime? pointInTime,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var records = await _historicalRecordBusiness
                    .GetAllHistoricalRecords(projectId, dataSourceId, pointInTime, hideArchived);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing historical records: {exc}";
                _logger.LogError(message);
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
        [HttpGet("GetHistoricalRecord/{recordId}", Name = "api_get_a_historical_record")]
        public async Task<ActionResult<HistoricalRecordResponseDto>> GetHistoricalRecord(
            long recordId,
            [FromQuery] DateTime? pointInTime, 
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var record = await _historicalRecordBusiness
                    .GetHistoricalRecord(recordId, pointInTime, hideArchived);
                return Ok(record);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving historical record {recordId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get record history
        /// </summary>
        /// <param name="recordId">ID of record to search on </param>
        /// <returns>A list of previous record versions</returns>
        [HttpGet("GetRecordHistory/{recordId}", Name = "api_get_a_historical_record_history")]
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
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}

