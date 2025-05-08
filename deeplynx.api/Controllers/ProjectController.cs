using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectBusiness _projectBusiness;

        public ProjectController(IProjectBusiness projectBusiness)
        {
            _projectBusiness = projectBusiness;
        }

        [HttpGet("GetAllProjects")]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projects = await _projectBusiness.GetAllProjects();
                return Ok(projects);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing projects: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpGet("GetProject/{projectId}")]
        public async Task<IActionResult> GetProject(long projectId)
        {
            try
            {
                var project = await _projectBusiness.GetProject(projectId);
                return Ok(project);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving project {projectId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpPost("CreateProject")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectRequestDto project)
        {
            try
            {
                var createdProject = await _projectBusiness.CreateProject(project);
                return CreatedAtAction(nameof(GetProject), new { projectId = createdProject.Id }, createdProject);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating project: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpPut("UpdateProject/{projectId}")]
        public async Task<IActionResult> UpdateProject(long projectId, [FromBody] ProjectRequestDto project)
        {
            try
            {
                var updatedProject = await _projectBusiness.UpdateProject(projectId, project);
                return Ok(updatedProject);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating project {projectId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [HttpDelete("DeleteProject/{projectId}")]
        public async Task<IActionResult> DeleteProject(long projectId)
        {
            try
            {
                await _projectBusiness.DeleteProject(projectId);
                return Ok(new { message = $"Deleted project {projectId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting project {projectId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}