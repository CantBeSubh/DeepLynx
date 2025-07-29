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
        /// Get all records
        /// </summary>
        /// <param name="projectId">Project ID which records are associated with</param>
        /// <param name="dataSourceId">Datasource ID which records are associated with</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived records from the result (Default true)</param>
        /// <returns>List of record response DTOs</returns>
        [HttpGet("GetAllRecords")]
        public async Task<ActionResult<IEnumerable<RecordResponseDto>>> GetAllRecords(
            long projectId,
            [FromQuery] long? dataSourceId,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var records = await _recordBusiness.GetAllRecords(projectId, dataSourceId, hideArchived);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing records: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get a record
        /// </summary>
        /// <param name="projectId">Project ID which record is associated with</param>
        /// <param name="recordId">Datasource ID which record is associated with</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived records from the result (Default true)</param>
        /// <returns>Record response DTO</returns>
        [HttpGet("GetRecord/{recordId}")]
        public async Task<ActionResult<RecordResponseDto>> GetRecord(
            long projectId, 
            long recordId, 
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var record = await _recordBusiness.GetRecord(projectId, recordId, hideArchived);
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
        /// Create a record
        /// </summary>
        /// <param name="projectId">Project ID which record is associated with</param>
        /// <param name="dataSourceId">Datasource ID which record is associated with</param>
        /// <param name="dto">Record request DTO</param>
        /// <returns>Record response DTO</returns>
        [HttpPost("CreateRecord")]
        public async Task<ActionResult<RecordResponseDto>> CreateRecord(
            long projectId, 
            [FromQuery] long dataSourceId,
            [FromBody] CreateRecordRequestDto dto)
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
        /// Create many records
        /// </summary>
        /// <param name="projectId">Project ID which record is associated with</param>
        /// <param name="dataSourceId">Datasource ID which record is associated with</param>
        /// <param name="records">List of record request DTOs</param>
        /// <returns>Record response DTO</returns>
        [HttpPost("BulkCreateRecords")]
        public async Task<ActionResult<List<RecordResponseDto>>> BulkCreateRecords(
            long projectId, 
            [FromQuery] long dataSourceId,
            [FromBody] List<CreateRecordRequestDto> records)
        {
            try
            {
                var newRecords = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating records: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Update a record
        /// </summary>
        /// <param name="projectId">Project ID which record is associated with</param>
        /// <param name="recordId">ID of record to be upated</param>
        /// <param name="dto">Record request DTO</param>
        /// <returns>Record response DTO</returns>
        [HttpPut("UpdateRecord/{recordId}")]
        public async Task<ActionResult<RecordResponseDto>> UpdateRecord(
            long projectId,
            long recordId,
            [FromBody] UpdateRecordRequestDto dto)
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
        /// Delete a record 
        /// </summary>
        /// <param name="recordId">The ID of the record to delete.</param>
        /// <param name="projectId">The ID of the project to which the record belongs.</param>
        /// <returns>A message stating the record was successfully deleted.</returns>
        [HttpDelete("DeleteRecord/{recordId}")]
        public async Task<IActionResult> DeleteRecord(long projectId, long recordId)
        {
            try
            {
                await _recordBusiness.DeleteRecord(projectId, recordId);
                return Ok(new { message = $"Deleted record {recordId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
            
        /// <summary>
        /// Archive a record
        /// </summary>
        /// <param name="recordId">The ID of the record to archive.</param>
        /// <param name="projectId">The ID of the project to which the record belongs.</param>
        /// <returns>A message stating the record was successfully archived.</returns>
        [HttpDelete("ArchiveRecord/{recordId}")]
        public async Task<IActionResult> ArchiveRecord(long projectId, long recordId)
        {
            try
            {
                await _recordBusiness.ArchiveRecord(projectId, recordId);
                return Ok(new { message = $"Archived record {recordId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unarchive a record
        /// </summary>
        /// <param name="recordId">The ID of the record to unarchive.</param>
        /// <param name="projectId">The ID of the project to which the record belongs.</param>
        /// <returns>A message stating the record was successfully unarchived.</returns>
        [HttpPut("UnarchiveRecord/{recordId}")]
        public async Task<IActionResult> UnarchiveRecord(long projectId, long recordId)
        {
            try
            {
                await _recordBusiness.UnarchiveRecord(projectId, recordId);
                return Ok(new { message = $"Unarchived record {recordId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Attach a tag to a record
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="recordId">The ID of the record.</param>
        /// <param name="tagId">The ID of the tag.</param>
        /// <returns>A message stating the tag was successfully attached to the record.</returns>
        [HttpPost("AttachTag/{recordId}")]
        public async Task<IActionResult> AttachTag(long projectId, long recordId, [FromQuery] long tagId)
        {
            try
            {
                await _recordBusiness.AttachTag(projectId, recordId, tagId);
                return Ok(new { message = $"Tag {tagId} attached to record {recordId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while attaching tag {tagId} to record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unattach a tag to a record
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="recordId">The ID of the record.</param>
        /// <param name="tagId">The ID of the tag.</param>
        /// <returns>A message stating the tag was successfully unattached from the record.</returns>
        [HttpPost("UnattachTag/{recordId}")]
        public async Task<IActionResult> UnattachTag(long projectId, long recordId, [FromQuery] long tagId)
        {
            try
            {
                await _recordBusiness.UnattachTag(projectId, recordId, tagId);
                return Ok(new { message = $"Tag {tagId} unattached from record {recordId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unattaching tag {tagId} from record {recordId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}

