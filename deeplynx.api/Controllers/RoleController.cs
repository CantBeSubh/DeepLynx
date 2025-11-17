using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("roles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleBusiness _roleBusiness;
        private readonly ILogger<RoleController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleController"/> class
        /// </summary>
        /// <param name="roleBusiness">The business logic interface for handling Role operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public RoleController(IRoleBusiness roleBusiness, ILogger<RoleController> logger)
        {
            _roleBusiness = roleBusiness;
            _logger = logger;
        }

        /// <summary>
        /// List Roles
        /// </summary>
        /// <param name="projectId">(optional) ID of the project across which to search</param>
        /// <param name="organizationId">(optional) ID of the organization across which to search</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived roles</param>
        /// <returns></returns>
        [HttpGet("GetAllRoles", Name = "api_get_all_roles")]
        public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetAllRoles(
            [FromQuery] long organizationId, 
            [FromQuery] long? projectId = null,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var roles = await _roleBusiness.GetAllRoles(organizationId, projectId, hideArchived);
                return Ok(roles);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing roles: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Fetch Role by ID
        /// </summary>
        /// <param name="roleId">ID of role</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived roles</param>
        /// <returns></returns>
        [HttpGet("GetRole/{roleId}", Name = "api_get_role")]
        public async Task<ActionResult<RoleResponseDto>> GetRole(
            long roleId, [FromQuery] bool hideArchived = true)
        {
            try
            {
                var role = await _roleBusiness.GetRole(roleId, hideArchived);
                return Ok(role);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving role {roleId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create a Role
        /// </summary>
        /// <param name="dto">Data structure of role to create</param>
        /// <param name="projectId">(use this or org ID) ID of the project to which the role belongs</param>
        /// <param name="organizationId">(use this or project ID) ID of the organization to which the role belongs</param>
        /// <returns></returns>
        [HttpPost("CreateRole", Name = "api_create_role")]
        public async Task<ActionResult<RoleResponseDto>> CreateRole(
            [FromBody] CreateRoleRequestDto dto,
            [FromQuery] long organizationId,
            [FromQuery] long? projectId
            )
        {
            try
            {
                var role = await _roleBusiness.CreateRole(dto, organizationId, projectId);
                return Ok(role);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating role: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update a Role
        /// </summary>
        /// <param name="roleId">ID of the role</param>
        /// <param name="dto">Fields to update</param>
        /// <returns></returns>
        [HttpPut("UpdateRole/{roleId}", Name = "api_update_role")]
        public async Task<ActionResult<RoleResponseDto>> UpdateRole(
            long roleId,
            [FromBody] UpdateRoleRequestDto dto)
        {
            try
            {
                var role = await _roleBusiness.UpdateRole(roleId, dto);
                return Ok(role);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating role {roleId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="roleId">ID of the role to hard delete</param>
        /// <returns></returns>
        [HttpDelete("DeleteRole/{roleId}", Name = "api_delete_role")]
        public async Task<ActionResult> DeleteRole(long roleId)
        {
            try
            {
                await _roleBusiness.DeleteRole(roleId);
                return Ok(new { message = $"Deleted role {roleId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting role {roleId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Archive a role
        /// </summary>
        /// <param name="roleId">ID of the role</param>
        /// <returns></returns>
        [HttpDelete("ArchiveRole/{roleId}", Name = "api_archive_role")]
        public async Task<ActionResult> ArchiveRole(long roleId)
        {
            try
            {
                await _roleBusiness.ArchiveRole(roleId);
                return Ok(new { message = $"Archived role {roleId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving role {roleId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Unarchive a Role
        /// </summary>
        /// <param name="roleId">ID of the role</param>
        /// <returns></returns>
        [HttpPut("UnarchiveRole/{roleId}", Name = "api_unarchive_role")]
        public async Task<ActionResult> UnarchiveRole(long roleId)
        {
            try
            {
                await _roleBusiness.UnarchiveRole(roleId);
                return Ok(new { message = $"Unarchived role {roleId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving role {roleId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// List Permissions by Role
        /// </summary>
        /// <param name="roleId">ID of the role</param>
        /// <returns></returns>
        [HttpGet("GetPermissionsByRole/{roleId}", Name = "api_get_permissions_by_role")]
        public async Task<ActionResult<IEnumerable<PermissionResponseDto>>> GetPermissionsByRole(long roleId)
        {
            try
            {
                var permissions = await _roleBusiness.GetPermissionsByRole(roleId);
                return Ok(permissions);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving permissions for role {roleId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Add permission to role
        /// </summary>
        /// <param name="roleId">ID of the role</param>
        /// <param name="permissionId">ID of the permission to be added</param>
        /// <returns></returns>
        [HttpPost("AddPermissionToRole", Name = "api_add_permission_to_role")]
        public async Task<ActionResult> AddPermissionToRole(
            [FromQuery] long roleId, 
            [FromQuery] long permissionId)
        {
            try
            {
                await _roleBusiness.AddPermissionToRole(roleId, permissionId);
                return Ok(new { message = $"Added permission {permissionId} to role {roleId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while adding permission {permissionId} to role {roleId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Remove permission from role
        /// </summary>
        /// <param name="roleId">ID of the role to remove from</param>
        /// <param name="permissionId">ID of permission to be removed</param>
        /// <returns></returns>
        [HttpDelete("RemovePermissionFromRole", Name = "api_remove_permission_from_role")]
        public async Task<ActionResult> RemovePermissionFromRole(
            [FromQuery] long roleId, 
            [FromQuery] long permissionId)
        {
            try
            {
                await _roleBusiness.RemovePermissionFromRole(roleId, permissionId);
                return Ok(new { message = $"Removed permission {permissionId} from role {roleId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while removing permission {permissionId} from role {roleId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Set all permissions for a role (replaces existing permissions)
        /// </summary>
        /// <param name="roleId">ID of the role</param>
        /// <param name="permissionIds">Array of permission IDs to assign to the role</param>
        /// <returns></returns>
        [HttpPut("SetPermissionsForRole/{roleId}", Name = "api_set_permissions_for_role")]
        public async Task<ActionResult> SetPermissionsForRole(
            long roleId,
            [FromBody] long[] permissionIds)
        {
            try
            {
                await _roleBusiness.SetPermissionsForRole(roleId, permissionIds);
                return Ok(new { message = $"Set permissions for role {roleId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while setting permissions for role {roleId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}