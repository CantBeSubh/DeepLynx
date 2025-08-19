using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    
    /// <summary>
    /// Controller for managing classes.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints to create, update, delete, and retrieve class information.
    /// </remarks>

    [ApiController]
    [Route("api/projects/{projectId}/classes")]
    public class ClassController : ControllerBase
    {
        private readonly IClassBusiness _classBusiness;
        private readonly ILogger<ClassController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassController"/> class
        /// </summary>
        /// <param name="classBusiness">The business logic interface for handling class operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public ClassController(IClassBusiness classBusiness, ILogger<ClassController> logger)
        {
            _classBusiness = classBusiness;
            _logger = logger;
        }
        /// <summary>
        /// Get all classes
        /// </summary>
        /// <param name="projectId">The ID of the project to which the class belongs</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result (Default true)</param>
        /// <returns>List of class response DTOs</returns>
        [HttpGet("GetAllClasses", Name = "api_get_all_classes")]
        public async Task<ActionResult<IEnumerable<ClassResponseDto>>> GetAllClasses(
            long projectId, 
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var classes = await _classBusiness.GetAllClasses(projectId, hideArchived);
                return Ok(classes);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching all classes.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }
        /// <summary>
        /// Get a class
        /// </summary>
        /// <param name="projectId">The ID of the project to which the class belongs</param>
        /// <param name="classId">The ID of the class to retrieve</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result (Default true)</param>
        /// <returns>Class response DTO</returns>
        [HttpGet("GetClass/{classId}", Name = "api_get_a_class")]
        public async Task<ActionResult<ClassResponseDto>> GetClass(
            long projectId, 
            long classId, 
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var classes = await _classBusiness.GetClass(projectId, classId, hideArchived);
                return Ok(classes);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching this class {classId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }


        /// <summary>
        /// Create a class
        /// </summary>
        /// <param name="projectId">The ID of the project to which the class belongs</param>
        /// <param name="dto">The request DTO for classes</param>
        /// <returns>Class response DTOs</returns>
        [HttpPost("CreateClass", Name = "api_create_a_class")]
        public async Task<ActionResult<ClassResponseDto>> CreateClass(long projectId,
            [FromBody] CreateClassRequestDto dto)
        {
            try
            {
                var newClass = await _classBusiness.CreateClass(projectId, dto);
                return Ok(newClass);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating this class.: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Create many classes
        /// </summary>
        /// <param name="projectId">The ID of the project to which the class belongs</param>
        /// <param name="classes">List of request DTOs for classes</param>
        /// <returns>Bulk class response DTOs</returns>
        [HttpPost("BulkCreateClasses", Name = "api_create_many_classes")]
        public async Task<ActionResult<List<ClassResponseDto>>> BulkCreateClasses(
            long projectId,
            [FromBody] List<CreateClassRequestDto> classes)
        {
            try
            {
                var newClasses = await _classBusiness.BulkCreateClasses(projectId, classes);
                return Ok(newClasses);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating these classes: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Update a class
        /// </summary>
        /// <param name="projectId">The ID of the project to which the class belongs</param>
        /// /// <param name="classId">The ID of the class to update</param>
        /// <param name="dto">The request DTO for the class</param>
        /// <returns>Class response DTO</returns>
        [HttpPut("UpdateClass/{classId}", Name = "api_update_a_class")]
        public async Task<ActionResult<ClassResponseDto>> UpdateClass(long projectId, long classId, [FromBody] UpdateClassRequestDto dto)
        {
            try
            {
                var updatedClass = await _classBusiness.UpdateClass(projectId, classId, dto);
                return Ok(updatedClass);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while updating this class {classId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete a class
        /// </summary>
        /// <param name="classId">The ID of the class to delete.</param>
        /// <param name="projectId">The ID of the project to which the class belongs.</param>
        /// <returns>A message stating the class was successfully deleted.</returns>
        [HttpDelete("DeleteClass/{classId}", Name = "api_delete_a_class")]
        public async Task<IActionResult> DeleteClass(long projectId, long classId)
        {
            try
            {
                await _classBusiness.DeleteClass(projectId, classId);
                return Ok(new { message = $"Deleted class {classId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting class {classId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive a class
        /// </summary>
        /// <param name="classId">The ID of the class to archive.</param>
        /// <param name="projectId">The ID of the project to which the class belongs.</param>
        /// <returns>A message stating the class was successfully archived.</returns>
        [HttpDelete("ArchiveClass/{classId}", Name = "api_archive_a_class")]
        public async Task<IActionResult> ArchiveClass(long projectId, long classId)
        {
            try
            {
                await _classBusiness.ArchiveClass(projectId, classId);
                return Ok(new { message = $"Archived class {classId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving class {classId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unarchive a class
        /// </summary>
        /// <param name="classId">The ID of the class to unarchive.</param>
        /// <param name="projectId">The ID of the project to which the class belongs.</param>
        /// <returns>A message stating the class was successfully unarchived.</returns>
        [HttpPut("UnarchiveClass/{classId}", Name = "api_unarchive_a_class")]
        public async Task<IActionResult> UnarchiveClass(long projectId, long classId)
        {
            try
            {
                await _classBusiness.UnarchiveClass(projectId, classId);
                return Ok(new { message = $"Unarchived class {classId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving class {classId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}