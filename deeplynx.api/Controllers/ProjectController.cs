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
        private readonly ILogger<ProjectController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectController"/> class
        /// </summary>
        /// <param name="projectBusiness">The business logic interface for handling project operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public ProjectController(IProjectBusiness projectBusiness, ILogger<ProjectController> logger)
        {
            _projectBusiness = projectBusiness;
            _logger = logger;
        }

        /// <summary>
        /// Get all projects
        /// </summary>
        /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
        /// <returns>A list of projects</returns>
        /// TODO: only list projects which the requesting user has access to once auth middleware is implemented
        [HttpGet("GetAllProjects", Name = "api_get_all_projects")]
        public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetAllProjects([FromQuery] bool hideArchived = true)
        {
            try
            {
                var projects = await _projectBusiness.GetAllProjects(hideArchived);
                return Ok(projects);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing projects: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get a project
        /// </summary>
        /// <param name="projectId">THe ID by which to retrieve the project</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
        /// <returns>The given project to return</returns>
        [HttpGet("GetProject/{projectId}", Name = "api_get_a_project")]
        public async Task<ActionResult<ProjectResponseDto>> GetProject(
            long projectId,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var project = await _projectBusiness.GetProject(projectId, hideArchived);
                return Ok(project);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving project {projectId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create a project
        /// </summary>
        /// <param name="dto">A data transfer object with details on the new project to be created.</param>
        /// <returns>The new project which was just created.</returns>
        [HttpPost("CreateProject", Name = "api_create_a_project")]
        public async Task<ActionResult<ProjectResponseDto>> CreateProject([FromBody] CreateProjectRequestDto dto)
        {
            try
            {
                var project = await _projectBusiness.CreateProject(dto);
                return Ok(project);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating project: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update a project
        /// </summary>
        /// <param name="projectId">The ID of the project to update</param>
        /// <param name="dto">A data transfer object with details on the project to be updated.</param>
        /// <returns>The project which was just updated.</returns>
        [HttpPut("UpdateProject/{projectId}", Name = "api_update_a_project")]
        public async Task<ActionResult<ProjectResponseDto>> UpdateProject(long projectId, [FromBody] UpdateProjectRequestDto dto)
        {
            try
            {
                var project = await _projectBusiness.UpdateProject(projectId, dto);
                return Ok(project);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating project {projectId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete a project
        /// </summary>
        /// <param name="projectId">ID of the project to delete.</param>
        /// <returns>Boolean true on successful deletion.</returns>
        [HttpDelete("DeleteProject/{projectId}", Name = "api_delete_a_project")]
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
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Archive a project
        /// </summary>
        /// <param name="projectId">ID of the project to archive.</param>
        /// <returns>A message stating the project was successfully archived.</returns>
        [HttpDelete("ArchiveProject/{projectId}", Name = "api_archive_a_project")]
        public async Task<IActionResult> ArchiveProject(long projectId)
        {
            try
            {
                await _projectBusiness.ArchiveProject(projectId);
                return Ok(new { message = $"Archived project {projectId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving project {projectId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unarchive a project
        /// </summary>
        /// <param name="projectId">ID of the project to unarchive.</param>
        /// <returns>A message stating the project was successfully unarchived.</returns>
        [HttpPut("UnarchiveProject/{projectId}", Name = "api_unarchive_a_project")]
        public async Task<IActionResult> UnarchiveProject(long projectId)
        {
            try
            {
                await _projectBusiness.UnarchiveProject(projectId);
                return Ok(new { message = $"Unarchived project {projectId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving project {projectId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Get project stats 
        /// </summary>
        /// <param name="projectId">ID of the project to display stats about.</param>
        /// <returns>Project stats</returns>
        [HttpGet("ProjectStats/{projectId}", Name = "api_get_a_projects_stats")]
        public async Task<ActionResult<ProjectStatResponseDto>> ProjectStats(long projectId)
        {
            try
            {
                var stats = await _projectBusiness.GetProjectStats(projectId);
                return Ok(stats);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving project {projectId} stats: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Retrieves all records for multiple projects.
        /// </summary>
        /// <param name="projects">Array of project ids whose records are to be retrieved</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
        /// <returns>List of record response DTOs</returns>
        [HttpGet("MultiProjectRecords", Name = "api_multiproject_records")]
        public async Task<ActionResult<IEnumerable<RecordResponseDto>>> GetMultiProjectRecords(
            [FromQuery]long[] projects,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var records = await _projectBusiness.GetMultiProjectRecords(projects, hideArchived);
                return Ok(records);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing records: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}