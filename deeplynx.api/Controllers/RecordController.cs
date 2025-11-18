using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[ApiController]
[Route("organizations/{organizationId}/projects/{projectId}/records")]
[Authorize]
public class RecordController : ControllerBase
{
    private readonly ILogger<RecordController> _logger;
    private readonly IRecordBusiness _recordBusiness;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecordController" /> class
    /// </summary>
    /// <param name="recordBusiness">The business logic interface for handling record operations.</param>
    /// <param name="logger">Error/Info logging interface for database log table.</param>
    public RecordController(IRecordBusiness recordBusiness, ILogger<RecordController> logger)
    {
        _recordBusiness = recordBusiness;
        _logger = logger;
    }

    /// <summary>
    ///     Get all records
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">Project ID which records are associated with</param>
    /// <param name="dataSourceId">Datasource ID which records are associated with</param>
    /// <param name="fileType">File extension to filter by (e.g., pdf, png, jpg) - leading dot is optional and will be removed</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result (Default true)</param>
    /// <returns>List of record response DTOs</returns>
    [HttpGet("GetAllRecords", Name = "api_get_all_records")]
    public async Task<ActionResult<IEnumerable<RecordResponseDto>>> GetAllRecords(
        long organizationId,
        long projectId,
        [FromQuery] long? dataSourceId,
        [FromQuery] string? fileType,
        [FromQuery] bool hideArchived = true)
    {
        try
        {
            var records = await _recordBusiness.GetAllRecords(projectId, dataSourceId, hideArchived, fileType);
            return Ok(records);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while listing records: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Get all records that have every given tagId.
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">Project ID which records are associated with</param>
    /// <param name="tagIds">The list of Ids to filter records by - records must contain all Ids in the list</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result (Default true)</param>
    /// <returns>List of record response DTOs</returns>
    [HttpGet("GetRecordsByTags", Name = "api_get_records_by_tags")]
    public async Task<ActionResult<IEnumerable<RecordResponseDto>>> GetRecordsByTags(
        long organizationId,
        long projectId,
        [FromQuery] long[] tagIds,
        [FromQuery] bool hideArchived = true)
    {
        try
        {
            var records = await _recordBusiness.GetRecordsByTags(projectId, tagIds, hideArchived);
            return Ok(records);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while listing records by tags: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Get a record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">Project ID which record is associated with</param>
    /// <param name="recordId">Datasource ID which record is associated with</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result (Default true)</param>
    /// <returns>Record response DTO</returns>
    [HttpGet("{recordId}/GetRecord", Name = "api_get_a_record")]
    public async Task<ActionResult<RecordResponseDto>> GetRecord(
        long organizationId,
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
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Create a record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">Project ID which record is associated with</param>
    /// <param name="dataSourceId">Datasource ID which record is associated with</param>
    /// <param name="dto">Record request DTO</param>
    /// <returns>Record response DTO</returns>
    [HttpPost("CreateRecord", Name = "api_create_a_record")]
    public async Task<ActionResult<RecordResponseDto>> CreateRecord(
        long organizationId,
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
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Create many records
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">Project ID which record is associated with</param>
    /// <param name="dataSourceId">Datasource ID which record is associated with</param>
    /// <param name="records">List of record request DTOs</param>
    /// <returns>Record response DTO</returns>
    [HttpPost("BulkCreateRecords", Name = "api_create_many_records")]
    public async Task<ActionResult<List<RecordResponseDto>>> BulkCreateRecords(
        long organizationId,
        long projectId,
        [FromQuery] long dataSourceId,
        [FromBody] List<CreateRecordRequestDto> records)
    {
        try
        {
            var newRecords = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);
            return Ok(newRecords);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while creating records: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Update a record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">Project ID which record is associated with</param>
    /// <param name="recordId">ID of record to be upated</param>
    /// <param name="dto">Record request DTO</param>
    /// <returns>Record response DTO</returns>
    [HttpPut("{recordId}/UpdateRecord", Name = "api_update_a_record")]
    public async Task<ActionResult<RecordResponseDto>> UpdateRecord(
        long organizationId,
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
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Delete a record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record to delete</param>
    /// <returns>A message stating the record was successfully deleted.</returns>
    [HttpDelete("{recordId}/DeleteRecord", Name = "api_delete_a_record")]
    public async Task<IActionResult> DeleteRecord(
        long organizationId,
        long projectId,
        long recordId)
    {
        try
        {
            await _recordBusiness.DeleteRecord(projectId, recordId);
            return Ok(new { message = $"Deleted record {recordId}" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting record {recordId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Archive a record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record to archive</param>
    /// <returns>A message stating the record was successfully archived.</returns>
    [HttpDelete("{recordId}/ArchiveRecord", Name = "api_archive_a_record")]
    public async Task<IActionResult> ArchiveRecord(
        long organizationId,
        long projectId,
        long recordId)
    {
        try
        {
            await _recordBusiness.ArchiveRecord(projectId, recordId);
            return Ok(new { message = $"Archived record {recordId}" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while archiving record {recordId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Unarchive a record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record to unarchive</param>
    /// <returns>A message stating the record was successfully unarchived.</returns>
    [HttpPut("{recordId}/UnarchiveRecord", Name = "api_unarchive_a_record")]
    public async Task<IActionResult> UnarchiveRecord(
        long organizationId,
        long projectId,
        long recordId)
    {
        try
        {
            await _recordBusiness.UnarchiveRecord(projectId, recordId);
            return Ok(new { message = $"Unarchived record {recordId}" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while unarchiving record {recordId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Attach a tag to a record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record</param>
    /// <param name="tagId">The ID of the tag</param>
    /// <returns>A message stating the tag was successfully attached to the record.</returns>
    [HttpPost("{recordId}/AttachTag", Name = "api_attach_a_tag")]
    public async Task<IActionResult> AttachTag(
        long organizationId,
        long projectId,
        long recordId,
        [FromQuery] long tagId)
    {
        try
        {
            await _recordBusiness.AttachTag(projectId, recordId, tagId);
            return Ok(new { message = $"Tag {tagId} attached to record {recordId}" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while attaching tag {tagId} to record {recordId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Unattach a tag to a record
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the project belongs</param>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record</param>
    /// <param name="tagId">The ID of the tag</param>
    /// <returns>A message stating the tag was successfully unattached from the record.</returns>
    [HttpPost("{recordId}/UnattachTag", Name = "api_unattach_a_tag")]
    public async Task<IActionResult> UnattachTag(
        long organizationId,
        long projectId,
        long recordId,
        [FromQuery] long tagId)
    {
        try
        {
            await _recordBusiness.UnattachTag(projectId, recordId, tagId);
            return Ok(new { message = $"Tag {tagId} unattached from record {recordId}" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while unattaching tag {tagId} from record {recordId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}