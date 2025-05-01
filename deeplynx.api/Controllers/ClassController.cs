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

        [HttpGet]
        public async Task<IActionResult> GetAllClasses(long projectId)
        {
            var classes = await _classBusiness.GetAllClasses(projectId);
            return Ok(classes);
        }

        [HttpGet("{classId}")]
        public async Task<IActionResult> GetClass(long projectId, long classId)
        {
            var classes = await _classBusiness.GetClass(projectId, classId);
            return Ok(classes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass(long projectId,
            [FromBody] ClassRequestDto dto)
        {
            var newClass = await _classBusiness.CreateClass(projectId, dto);
            return Ok(newClass);
        }

        [HttpPut("{classId}")]
        public async Task<IActionResult> UpdateClass(long projectId, long classId, [FromBody] ClassRequestDto dto)
        {
            var updatedClass = await _classBusiness.UpdateClass(projectId, classId, dto);
            return Ok(updatedClass);
        }

        [HttpDelete("{classId}")]
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
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the class." });
            }
        }
    }
}