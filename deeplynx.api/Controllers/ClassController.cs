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

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassController"/> class
        /// </summary>
        /// <param name="classBusiness">The business logic interface for handling class operations.</param>
        public ClassController(IClassBusiness classBusiness)
        {
            _classBusiness = classBusiness;
        }
        /// <summary>
        /// Get all classes
        /// </summary>
        /// <param name="projectId">The ID of the project to which the class belongs</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result (Default true)</param>
        /// <returns>List of class response DTOs</returns>
        [HttpGet("GetAllClasses")]
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
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
        [HttpGet("GetClass/{classId}")]
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

        }


        /// <summary>
        /// Create a class
        /// </summary>
        /// <param name="projectId">The ID of the project to which the class belongs</param>
        /// <param name="dto">The request DTO for classes</param>
        /// <returns>Class response DTOs</returns>
        [HttpPost("CreateClass")]
        public async Task<ActionResult<ClassResponseDto>> CreateClass(long projectId,
            [FromBody] ClassRequestDto dto)
        {
            try
            {
                var newClass = await _classBusiness.CreateClass(projectId, dto);
                return Ok(newClass);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating this class.: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Create many classes
        /// </summary>
        /// <param name="projectId">The ID of the project to which the class belongs</param>
        /// <param name="classes">List of request DTOs for classes</param>
        /// <returns>Bulk class response DTOs</returns>
        [HttpPost("BulkCreateClasses")]
        public async Task<ActionResult<List<ClassResponseDto>>> BulkCreateClasses(
            long projectId,
            [FromBody] List<ClassRequestDto> classes)
        {
            try
            {
                var newClasses = await _classBusiness.BulkCreateClasses(projectId, classes);
                return Ok(newClasses);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while creating these classes: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
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
        [HttpPut("UpdateClass/{classId}")]
        public async Task<ActionResult<ClassResponseDto>> UpdateClass(long projectId, long classId, [FromBody] ClassRequestDto dto)
        {
            try
            {
                var updatedClass = await _classBusiness.UpdateClass(projectId, classId, dto);
                return Ok(updatedClass);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while updating this class {classId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete a class
        /// </summary>
        /// <param name="classId">The ID of the class to delete.</param>
        /// <param name="projectId">The ID of the project to which the class belongs.</param>
        /// <returns>A message stating the class was successfully deleted.</returns>
        [HttpDelete("DeleteClass/{classId}")]
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Archive a class
        /// </summary>
        /// <param name="classId">The ID of the class to archive.</param>
        /// <param name="projectId">The ID of the project to which the class belongs.</param>
        /// <returns>A message stating the class was successfully archived.</returns>
        [HttpDelete("ArchiveClass/{classId}")]
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unarchive a class
        /// </summary>
        /// <param name="classId">The ID of the class to unarchive.</param>
        /// <param name="projectId">The ID of the project to which the class belongs.</param>
        /// <returns>A message stating the class was successfully unarchived.</returns>
        [HttpPut("UnarchiveClass/{classId}")]
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
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}