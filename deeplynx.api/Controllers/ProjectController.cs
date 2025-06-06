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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectController"/> class
        /// </summary>
        /// <param name="projectBusiness">The business logic interface for handling project operations.</param>
        public ProjectController(IProjectBusiness projectBusiness)
        {
            _projectBusiness = projectBusiness;
        }

        /// <summary>
        /// Retrieves all projects
        /// </summary>
        /// <returns>A list of projects</returns>
        /// TODO: only list projects which the requesting user has access to once auth middleware is implemented
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

        /// <summary>
        /// Retrieves a specific project by ID
        /// </summary>
        /// <param name="projectId">THe ID by which to retrieve the project</param>
        /// <returns>The given project to return</returns>
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

        /// <summary>
        /// Creates a new project based on the data transfer object supplied.
        /// </summary>
        /// <param name="dto">A data transfer object with details on the new project to be created.</param>
        /// <returns>The new project which was just created.</returns>
        [HttpPost("CreateProject")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectRequestDto dto)
        {
            try
            {
                var project = await _projectBusiness.CreateProject(dto);
                return Ok(project);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating project: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Updates an existing project by ID
        /// </summary>
        /// <param name="projectId">The ID of the project to update</param>
        /// <param name="dto">A data transfer object with details on the project to be updated.</param>
        /// <returns>The project which was just updated.</returns>
        [HttpPut("UpdateProject/{projectId}")]
        public async Task<IActionResult> UpdateProject(long projectId, [FromBody] ProjectRequestDto dto)
        {
            try
            {
                var project = await _projectBusiness.UpdateProject(projectId, dto);
                return Ok(project);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating project {projectId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete a project by ID. This also cascades to downstream dependents.
        /// </summary>
        /// <param name="projectId">ID of the project to delete.</param>
        /// <param name="force">Boolean flag to force delete a project if true.</param>
        /// <returns>Boolean true on successful deletion.</returns>
        [HttpDelete("DeleteProject/{projectId}")]
        public async Task<IActionResult> DeleteProject(long projectId, [FromQuery] bool force = false)
        {
            try
            {
                await _projectBusiness.DeleteProject(projectId, force);
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