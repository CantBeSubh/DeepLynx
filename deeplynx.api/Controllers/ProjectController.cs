using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("projects")]
    [Authorize]
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
        /// <param name="organizationId">(Optional)ID of an organization to filter by</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
        /// <returns>A list of projects</returns>
        /// TODO: only list projects which the requesting user has access to once auth middleware is implemented
        [HttpGet("GetAllProjects", Name = "api_get_all_projects")]
        public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetAllProjects(
            [FromQuery] long? organizationId,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                // get user ID from the middleware context
                var currentUserId = UserContextStorage.UserId;
                var projects = await _projectBusiness
                    .GetAllProjects(currentUserId, organizationId, hideArchived);
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
        /// <param name="projectId">The ID by which to retrieve the project</param>
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
                var currentUserId = UserContextStorage.UserId;
                var project = await _projectBusiness.CreateProject(currentUserId, dto);
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
        
        /// <summary>
        /// List Project Members
        /// </summary>
        /// <param name="projectId">(Optional)ID of the project</param>
        /// <returns>A list of groups and users in the project, along with their roles</returns>
        [HttpGet("GetProjectMembers/{projectId}", Name = "api_get_project_members")]
        public async Task<ActionResult> GetProjectMembers(long projectId)
        {
            try
            {
                var members = await _projectBusiness.GetProjectMembers(projectId);
                return Ok(members);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing project members for project {projectId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Add User or Group to Project
        /// </summary>
        /// <param name="projectId">ID of project</param>
        /// <param name="roleId">(Optional) ID of member role</param>
        /// <param name="userId">ID of user if user is member</param>
        /// <param name="groupId">ID of group if group is member</param>
        /// <returns></returns>
        [HttpPost("AddMemberToProject", Name = "api_add_member_to_project")]
        public async Task<ActionResult> AddMemberToProject(
            [FromQuery] long projectId, [FromQuery] long? roleId, 
            [FromQuery] long? userId, [FromQuery] long? groupId)
        {
            try
            {
                await _projectBusiness.AddMemberToProject(projectId, roleId, userId, groupId);
                return Ok(new { message = $"Added member {userId ?? groupId} to project {projectId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while adding member {userId ?? groupId} to project {projectId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Update Member Role in Project
        /// </summary>
        /// <param name="projectId">ID of project</param>
        /// <param name="roleId">ID of role</param>
        /// <param name="userId">ID of user if user is member</param>
        /// <param name="groupId">ID of group if group is member</param>
        /// <returns></returns>
        [HttpPut("UpdateProjectMemberRole", Name = "api_update_project_member_role")]
        public async Task<ActionResult> UpdateProjectMemberRole(
            [FromQuery] long projectId, [FromQuery] long roleId, 
            [FromQuery] long? userId, [FromQuery] long? groupId)
        {
            try
            {
                await _projectBusiness.UpdateProjectMemberRole(projectId, roleId, userId, groupId);
                return Ok(new { message = $"Updated member role in project {projectId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating member role in project {projectId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Remove User or Group from Project
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="userId">ID of the user if user is member</param>
        /// <param name="groupId">ID of the group if group is member</param>
        /// <returns></returns>
        [HttpDelete("RemoveMemberFromProject", Name = "api_remove_member_from_project")]
        public async Task<ActionResult> RemoveMemberFromProject(
            [FromQuery] long projectId,
            [FromQuery] long? userId, 
            [FromQuery] long? groupId)
        {
            try
            {
                await _projectBusiness.RemoveMemberFromProject(projectId, userId, groupId);
                return Ok(new { message = $"Removed member from project {projectId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while removing member from project {projectId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}