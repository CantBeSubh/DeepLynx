using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/roles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleBusiness _roleBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleController"/> class
        /// </summary>
        /// <param name="roleBusiness">The business logic interface for handling role operations.</param>
        public RoleController(IRoleBusiness roleBusiness)
        {
            _roleBusiness = roleBusiness;
        }

        /// <summary>
        /// Retrieve all roles for a specified project.
        /// </summary>
        /// <param name="projectId">The ID of the project whose roles are to be retrieved.</param>
        /// <returns>A list of roles belonging to the project.</returns>
        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles(long projectId)
        {
            try
            {
                var roles = await _roleBusiness.GetAllRoles(projectId);
                return Ok(new { message = "Returned all roles", data = roles });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing roles: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Retrieves a specific role by its ID
        /// </summary>
        /// <param name="projectId">The ID of the project to which the role belongs.</param>
        /// <param name="roleId">The ID of the role to retrieve.</param>
        /// <returns>The role with its details.</returns>
        [HttpGet("GetRole/{roleId}")]
        public async Task<IActionResult> GetRole(long projectId, long roleId)
        {
            try
            {
                var role = await _roleBusiness.GetRole(projectId, roleId);
                return Ok(role);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving role {roleId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Creates a new role for a specified project.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the role belongs.</param>
        /// <param name="dto">The role data transfer object containing role details.</param>
        /// <returns>The created role with its details.</returns>
        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(
            long projectId, 
            [FromBody] RoleRequestDto dto)
        {
            try
            {
                var role = await _roleBusiness.CreateRole(projectId, dto);
                return Ok(role);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating role: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }   

        /// <summary>
        /// Updates an existing role for a specified project.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the role belongs.</param>
        /// <param name="roleId">The ID of the role to update.</param>
        /// <param name="dto">The role data transfer object containing updated role details.</param>
        /// <returns>The updated role with its details.</returns>
        [HttpPut("UpdateRole/{roleId}")]
        public async Task<IActionResult> UpdateRole(
            long projectId, 
            long roleId, 
            [FromBody] RoleRequestDto dto)
        {
            try
            {
                var role = await _roleBusiness.UpdateRole(projectId, roleId, dto);
                return Ok(role);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating role {roleId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Deletes a specific role by its ID for a specified project.
        /// </summary>
        /// <param name="projectId">THe ID of the role to which the tag belongs.</param>
        /// <param name="roleId">The ID of the role to delete.</param>
        /// <param name="force">Boolean indicating whether to force delete the role if true.</param>
        /// <returns>A message stating the role was successfully deleted.</returns>
        [HttpDelete("DeleteRole/{roleId}")]
        public async Task<IActionResult> DeleteRole(long projectId, long roleId, [FromQuery] bool force = false)
        {
            try
            {
                await (_roleBusiness).DeleteRole(projectId, roleId, force);
                return Ok(new { message = $"Deleted role {roleId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting role {roleId}: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }


    }
}