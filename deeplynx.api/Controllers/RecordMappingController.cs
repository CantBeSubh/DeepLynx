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

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordMappingController"/> class
        /// </summary>
        /// <param name="rMappingBusiness">The business logic interface for handling record mapping operations.</param>
        public RecordMappingController(IRecordMappingBusiness rMappingBusiness)
        {
            _rMappingBusiness = rMappingBusiness;
        }

        /// <summary>
        /// Get all record mapping
        /// </summary>
        /// <param name="projectId">The ID of the project whose mappings are to be retrieved</param>
        /// <param name="classId">(Optional) The ID of the class by which to filter mappings</param>
        /// <param name="tagId">(Optional) The ID of the tag by which to filter mappings</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived mappings from the result (Default true)</param>
        /// <returns>A list of record mappings based on the applied filters.</returns>
        [HttpGet("GetAllRecordMappings")]
        public async Task<ActionResult<IEnumerable<RecordMappingResponseDto>>> GetAllRecordMappings(
            long projectId, 
            [FromQuery] long? classId = null,
            [FromQuery] long? tagId = null,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var rMappings = await _rMappingBusiness
                    .GetAllRecordMappings(projectId, classId, tagId, hideArchived);
                return Ok(rMappings);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing all record mappings: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get a record mapping
        /// </summary>
        /// <param name="projectId">The ID of the project to which the record mapping belongs</param>
        /// <param name="mappingId">The ID whereby to fetch the record mapping</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived mappings from the result (Default true)</param>
        /// <returns>The record mapping associated with the given ID</returns>
        [HttpGet("GetRecordMapping/{mappingId}")]
        public async Task<ActionResult<RecordMappingResponseDto>> GetRecordMapping(
            long projectId, 
            long mappingId,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var rMapping = await _rMappingBusiness.GetRecordMapping(projectId, mappingId, hideArchived);
                return Ok(rMapping);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving record mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Create a record mapping
        /// </summary>
        /// <param name="projectId">The ID of the project to which the record mapping belongs</param>
        /// <param name="dto">The record mapping request data transfer object containing mapping details</param>
        /// <returns>The created record mapping</returns>
        [HttpPost("CreateRecordMapping")]
        public async Task<ActionResult<RecordMappingResponseDto>> CreateRecordMapping(long projectId, [FromBody] RecordMappingRequestDto dto)
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
        
        /// <summary>
        /// Update a record mapping 
        /// </summary>
        /// <param name="projectId">The ID of the project to which the record mapping belongs.</param>
        /// <param name="mappingId">The ID of the record mapping to update.</param>
        /// <param name="dto">The record mapping request data transfer object containing updated details.</param>
        /// <returns>The updated mapping response DTO with its details.</returns>
        [HttpPut("UpdateRecordMapping/{mappingId}")]
        public async Task<ActionResult<RecordMappingResponseDto>> UpdateRecordMapping(
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
        
        /// <summary>
        /// Delete a record mapping 
        /// </summary>
        /// <param name="mappingId">The ID of the record mapping to delete.</param>
        /// <param name="projectId">The ID of the project to which the mapping belongs.</param>
        /// <returns>A message stating the record mapping was successfully deleted.</returns>
        [HttpDelete("DeleteRecordMapping/{mappingId}")]
        public async Task<IActionResult> DeleteRecordMapping(long projectId, long mappingId)
        {
            try
            {
                await _rMappingBusiness.DeleteRecordMapping(projectId, mappingId);
                return Ok(new { message = $"Deleted record mapping {mappingId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting record mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive a record mapping 
        /// </summary>
        /// <param name="mappingId">The ID of the record mapping to archive.</param>
        /// <param name="projectId">The ID of the project to which the mapping belongs.</param>
        /// <returns>A message stating the record mapping was successfully archived.</returns>
        [HttpDelete("ArchiveRecordMapping/{mappingId}")]
        public async Task<IActionResult> ArchiveRecordMapping(long projectId, long mappingId)
        {
            try
            {
                await _rMappingBusiness.ArchiveRecordMapping(projectId, mappingId);
                return Ok(new { message = $"Archived record mapping {mappingId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving record mapping {mappingId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}