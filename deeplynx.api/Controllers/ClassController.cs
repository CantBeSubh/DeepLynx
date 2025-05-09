using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("projects/{projectId}/classes")]
    public class ClassController : ControllerBase
    {
        private readonly IClassBusiness _classBusiness;

        public ClassController(IClassBusiness classBusiness)
        {
            _classBusiness = classBusiness;
        }
        /// <summary>
        /// Get all classes from DB
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("GetAllClasses")]
        public async Task<IActionResult> GetAllClasses(long projectId)
        {
            try
            {
                var classes = await _classBusiness.GetAllClasses(projectId);
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
        /// <param name="projectId"></param>
        /// <param name="classId"></param>
        /// <returns></returns>
        [HttpGet("GetClass/{classId}")]
        public async Task<IActionResult> GetClass(long projectId, long classId)
        {
            try
            {
                var classes = await _classBusiness.GetClass(projectId, classId);
                return Ok(classes);
            }  catch (Exception exc)
            {
                var message = $"An unexpected error occurred while fetching this class {classId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
           
        }

        
        /// <summary>
        /// Create a class
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("CreateClass")]
        public async Task<IActionResult> CreateClass(long projectId,
            [FromBody] ClassRequestDto dto)
        {
            try
            {
                var newClass = await _classBusiness.CreateClass(projectId, dto);
                return Ok(newClass);
            }
            catch (Exception exc){
                var message = $"An unexpected error occurred while creating this class.: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpPut("UpdateClass/{classId}")]
        public async Task<IActionResult> UpdateClass(long projectId, long classId, [FromBody] ClassRequestDto dto)
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
        /// <param name="projectId"></param>
        /// <param name="classId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteClass/{classId}")]
        public async Task<IActionResult> DeleteClass(long projectId, long classId)
        {
            try
            {
                var result = await _classBusiness.DeleteClass(projectId, classId);

                return result
                    ? Ok(new { message = "Class successfully deleted." })
                    : BadRequest(new { error = "Class could not be deleted. It may have already been deleted." });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                var statusCode = ex is KeyNotFoundException
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, new { error = ex.Message });
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while deleting this class {classId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}