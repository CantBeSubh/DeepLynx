using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionBusiness _permissionBusiness;
        private readonly ILogger<PermissionController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionController"/> class
        /// </summary>
        /// <param name="permissionBusiness">The business logic interface for handling Permission operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public PermissionController(IPermissionBusiness permissionBusiness, ILogger<PermissionController> logger)
        {
            _permissionBusiness = permissionBusiness;
            _logger = logger;
        }

        /// <summary>
        /// List Permissions
        /// </summary>
        /// <param name="labelId">(optional) ID of a sensitivity label to filter by</param>
        /// <param name="projectId">(optional) ID of a project to filter by</param>
        /// <param name="organizationId">(optional) ID of an organization to filter by</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived permissions</param>
        /// <returns></returns>
        [HttpGet("GetAllPermissions", Name = "api_get_all_permissions")]
        public async Task<ActionResult<IEnumerable<PermissionResponseDto>>> GetAllPermissions(
            [FromQuery] long? labelId = null,
            [FromQuery] long? projectId = null,
            [FromQuery] long? organizationId = null,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var permissions = await _permissionBusiness.GetAllPermissions(labelId, projectId, organizationId, hideArchived);
                return Ok(permissions);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing permissions: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Fetch Permission by ID
        /// </summary>
        /// <param name="permissionId">ID of permission</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived permissions</param>
        /// <returns></returns>
        [HttpGet("GetPermission/{permissionId}", Name = "api_get_permission")]
        public async Task<ActionResult<PermissionResponseDto>> GetPermission(
            long permissionId, [FromQuery] bool hideArchived = true)
        {
            try
            {
                var permission = await _permissionBusiness.GetPermission(permissionId, hideArchived);
                return Ok(permission);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving permission {permissionId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create a Permission
        /// </summary>
        /// <param name="dto">Data structure of permission to create</param>
        /// <param name="projectId">(use this or org ID) ID of the project to which the permission belongs</param>
        /// <param name="organizationId">(use this or org ID) ID of the organization to which the permission belongs</param>
        /// <returns></returns>
        [HttpPost("CreatePermission", Name = "api_create_permission")]
        public async Task<ActionResult<PermissionResponseDto>> CreatePermission(
            [FromBody] CreatePermissionRequestDto dto,
            [FromQuery] long? projectId = null,
            [FromQuery] long? organizationId = null)
        {
            try
            {
                var permission = await _permissionBusiness.CreatePermission(dto, projectId, organizationId);
                return Ok(permission);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating permission: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update a Permission
        /// </summary>
        /// <param name="permissionId">ID of the permission</param>
        /// <param name="dto">Fields to update</param>
        /// <returns></returns>
        [HttpPut("UpdatePermission/{permissionId}", Name = "api_update_permission")]
        public async Task<ActionResult<PermissionResponseDto>> UpdatePermission(
            long permissionId,
            [FromBody] UpdatePermissionRequestDto dto)
        {
            try
            {
                var permission = await _permissionBusiness.UpdatePermission(permissionId, dto);
                return Ok(permission);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating permission {permissionId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete a permission
        /// </summary>
        /// <param name="permissionId">ID of the permission to hard delete</param>
        /// <returns></returns>
        [HttpDelete("DeletePermission/{permissionId}", Name = "api_delete_permission")]
        public async Task<ActionResult> DeletePermission(long permissionId)
        {
            try
            {
                await _permissionBusiness.DeletePermission(permissionId);
                return Ok(new { message = $"Deleted permission {permissionId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting permission {permissionId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Archive a permission
        /// </summary>
        /// <param name="permissionId">ID of the permission</param>
        /// <returns></returns>
        [HttpDelete("ArchivePermission/{permissionId}", Name = "api_archive_permission")]
        public async Task<ActionResult> ArchivePermission(long permissionId)
        {
            try
            {
                await _permissionBusiness.ArchivePermission(permissionId);
                return Ok(new { message = $"Archived permission {permissionId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving permission {permissionId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Unarchive a Permission
        /// </summary>
        /// <param name="permissionId">ID of the permission</param>
        /// <returns></returns>
        [HttpPut("UnarchivePermission/{permissionId}", Name = "api_unarchive_permission")]
        public async Task<ActionResult> UnarchivePermission(long permissionId)
        {
            try
            {
                await _permissionBusiness.UnarchivePermission(permissionId);
                return Ok(new { message = $"Unarchived permission {permissionId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving permission {permissionId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}