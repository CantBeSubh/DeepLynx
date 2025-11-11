using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("organizations")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationBusiness _organizationBusiness;
        private readonly ILogger<OrganizationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationController"/> class
        /// </summary>
        /// <param name="organizationBusiness">The business logic interface for handling Organization operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public OrganizationController(IOrganizationBusiness organizationBusiness,
            ILogger<OrganizationController> logger)
        {
            _organizationBusiness = organizationBusiness;
            _logger = logger;
        }

        /// <summary>
        /// List all organizations
        /// </summary>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived orgs</param>
        /// <returns></returns>
        [HttpGet("GetAllOrganizations", Name = "api_get_all_organizations")]
        public async Task<ActionResult<IEnumerable<OrganizationResponseDto>>> GetAllOrganizations(
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var organizations = await _organizationBusiness
                    .GetAllOrganizations(hideArchived);
                return Ok(organizations);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing organizations: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Fetch Organization by ID
        /// </summary>
        /// <param name="organizationId">ID of organization</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived orgs</param>
        /// <returns></returns>
        [HttpGet("GetOrganization/{organizationId}", Name = "api_get_organization")]
        public async Task<ActionResult<OrganizationResponseDto>> GetOrganization(
            long organizationId, [FromQuery] bool hideArchived = true)
        {
            try
            {
                var organization = await _organizationBusiness.GetOrganization(organizationId, hideArchived);
                return Ok(organization);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving organization {organizationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create an Organization
        /// </summary>
        /// <param name="dto">Data structure of organization to create</param>
        /// <returns></returns>
        [HttpPost("CreateOrganization", Name = "api_create_organization")]
        public async Task<ActionResult<OrganizationResponseDto>> CreateOrganization(
            [FromBody] CreateOrganizationRequestDto dto)
        {
            try
            {
                var organization = await _organizationBusiness.CreateOrganization(dto);
                return Ok(organization);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating organization: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update an Organization
        /// </summary>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="dto">Fields to update</param>
        /// <returns></returns>
        [HttpPut("UpdateOrganization/{organizationId}", Name = "api_update_organization")]
        public async Task<ActionResult<OrganizationResponseDto>> UpdateOrganization(
            long organizationId,
            [FromBody] UpdateOrganizationRequestDto dto)
        {
            try
            {
                var organization = await _organizationBusiness.UpdateOrganization(organizationId, dto);
                return Ok(organization);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating organization {organizationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete an organization
        /// </summary>
        /// <param name="organizationId">ID of the organization to hard delete</param>
        /// <returns></returns>
        [HttpDelete("DeleteOrganization/{organizationId}", Name = "api_delete_organization")]
        public async Task<ActionResult> DeleteOrganization(long organizationId)
        {
            try
            {
                await _organizationBusiness.DeleteOrganization(organizationId);
                return Ok(new { message = $"Deleted organization {organizationId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting organization {organizationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Archive an organization
        /// </summary>
        /// <param name="organizationId">ID of the organization</param>
        /// <returns></returns>
        [HttpDelete("ArchiveOrganization/{organizationId}", Name = "api_archive_organization")]
        public async Task<ActionResult> ArchiveOrganization(long organizationId)
        {
            try
            {
                await _organizationBusiness.ArchiveOrganization(organizationId);
                return Ok(new { message = $"Archived organization {organizationId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving organization {organizationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Unarchive an Organization
        /// </summary>
        /// <param name="organizationId">ID of the organization</param>
        /// <returns></returns>
        [HttpPut("UnarchiveOrganization/{organizationId}", Name = "api_unarchive_organization")]
        public async Task<ActionResult> UnarchiveOrganization(long organizationId)
        {
            try
            {
                await _organizationBusiness.UnarchiveOrganization(organizationId);
                return Ok(new { message = $"Unarchived organization {organizationId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving organization {organizationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Add User to Organization
        /// </summary>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="userId">ID of the user to be added</param>
        /// <param name="isAdmin"></param>
        /// <returns></returns>
        [HttpPost("AddUserToOrganization", Name = "api_add_user_to_organization")]
        public async Task<ActionResult> AddUserToOrganization(
            [FromQuery] long organizationId, 
            [FromQuery] long userId,
            [FromQuery] bool isAdmin = false)
        {
            try
            {
                await _organizationBusiness.AddUserToOrganization(organizationId, userId, isAdmin);
                return Ok(new { message = $"Added user {userId} to organization {organizationId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while adding user {userId} to organization {organizationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Set Admin Status for Organization User
        /// </summary>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="userId">ID of the user</param>
        /// <param name="isAdmin">isAdmin status</param>
        /// <returns></returns>
        [HttpPut("SetOrganizationAdminStatus", Name = "api_update_organization_admin_status")]
        public async Task<ActionResult> SetOrganizationAdminStatus(
            [FromQuery] long organizationId, 
            [FromQuery] long userId,
            [FromQuery] bool isAdmin)
        {
            try
            {
                await _organizationBusiness.SetOrganizationAdminStatus(organizationId, userId, isAdmin);
                return Ok(new { message = $"Adjusted admin status for user {userId} in organization {organizationId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while setting admin status for user {userId} in organization {organizationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
        
        /// <summary>
        /// Remove user from organization
        /// </summary>
        /// <param name="organizationId">ID of the organization to remove from</param>
        /// <param name="userId">ID of user to be removed</param>
        /// <returns></returns>
        [HttpDelete("RemoveUserFromOrganization", Name = "api_remove_user_from_organization")]
        public async Task<ActionResult> RemoveUserFromOrganization(
            [FromQuery] long organizationId, 
            [FromQuery] long userId)
        {
            try
            {
                await _organizationBusiness.RemoveUserFromOrganization(organizationId, userId);
                return Ok(new { message = $"Removed user {userId} from organization {organizationId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while removing user {userId} from organization {organizationId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}