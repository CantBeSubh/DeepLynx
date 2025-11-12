using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("groups")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupBusiness _groupBusiness;
        private readonly ILogger<GroupController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupController"/> class
        /// </summary>
        /// <param name="groupBusiness">The business logic interface for handling Group operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public GroupController(IGroupBusiness groupBusiness, ILogger<GroupController> logger)
        {
            _groupBusiness = groupBusiness;
            _logger = logger;
        }

        /// <summary>
        /// List all groups within an organization
        /// </summary>
        /// <param name="organizationId">ID of the organization from which to list groups</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived groups</param>
        /// <returns></returns>
        [HttpGet("GetAllGroups", Name = "api_get_all_groups")]
        public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetAllGroups(
            [FromQuery] long organizationId,
            [FromQuery] bool hideArchived = true)
        {
            try
            {
                var groups = await _groupBusiness.GetAllGroups(organizationId, hideArchived);
                return Ok(groups);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while listing groups: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Fetch Group by ID
        /// </summary>
        /// <param name="groupId">ID of group</param>
        /// <param name="hideArchived">Flag indicating whether to hide or show archived groups</param>
        /// <returns></returns>
        [HttpGet("GetGroup/{groupId}", Name = "api_get_group")]
        public async Task<ActionResult<GroupResponseDto>> GetGroup(
            long groupId, [FromQuery] bool hideArchived = true)
        {
            try
            {
                var group = await _groupBusiness.GetGroup(groupId, hideArchived);
                return Ok(group);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving group {groupId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Get all members of a group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <returns>List of users in the group</returns>
        [HttpGet("GetGroupMembers/{groupId}", Name = "api_get_group_members")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetGroupMembers(long groupId)
        {
            try
            {
                var members = await _groupBusiness.GetGroupMembers(groupId);
                return Ok(members);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while retrieving members for group {groupId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Create a Group
        /// </summary>
        /// <param name="dto">Data structure of group to create</param>
        /// <param name="organizationId">ID of the organization to which the group belongs</param>
        /// <returns></returns>
        [HttpPost("CreateGroup", Name = "api_create_group")]
        public async Task<ActionResult<GroupResponseDto>> CreateGroup(
            [FromBody] CreateGroupRequestDto dto,
            [FromQuery] long organizationId)
        {
            try
            {
                var group = await _groupBusiness.CreateGroup(organizationId, dto);
                return Ok(group);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while creating group: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Update a Group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <param name="dto">Fields to update</param>
        /// <returns></returns>
        [HttpPut("UpdateGroup/{groupId}", Name = "api_update_group")]
        public async Task<ActionResult<GroupResponseDto>> UpdateGroup(
            long groupId,
            [FromBody] UpdateGroupRequestDto dto)
        {
            try
            {
                var group = await _groupBusiness.UpdateGroup(groupId, dto);
                return Ok(group);
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while updating group {groupId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Delete a group
        /// </summary>
        /// <param name="groupId">ID of the group to hard delete</param>
        /// <returns></returns>
        [HttpDelete("DeleteGroup/{groupId}", Name = "api_delete_group")]
        public async Task<ActionResult> DeleteGroup(long groupId)
        {
            try
            {
                await _groupBusiness.DeleteGroup(groupId);
                return Ok(new { message = $"Deleted group {groupId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting group {groupId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Archive a group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <returns></returns>
        [HttpDelete("ArchiveGroup/{groupId}", Name = "api_archive_group")]
        public async Task<ActionResult> ArchiveGroup(long groupId)
        {
            try
            {
                await _groupBusiness.ArchiveGroup(groupId);
                return Ok(new { message = $"Archived group {groupId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while archiving group {groupId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Unarchive a Group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <returns></returns>
        [HttpPut("UnarchiveGroup/{groupId}", Name = "api_unarchive_group")]
        public async Task<ActionResult> UnarchiveGroup(long groupId)
        {
            try
            {
                await _groupBusiness.UnarchiveGroup(groupId);
                return Ok(new { message = $"Unarchived group {groupId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while unarchiving group {groupId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Add user to group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <param name="userId">ID of the user to be added</param>
        /// <returns></returns>
        [HttpPost("AddUserToGroup", Name = "api_add_user_to_group")]
        public async Task<ActionResult> AddUserToGroup(
            [FromQuery] long groupId,
            [FromQuery] long userId)
        {
            try
            {
                await _groupBusiness.AddUserToGroup(groupId, userId);
                return Ok(new { message = $"Added user {userId} to group {groupId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while adding user {userId} to group {groupId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        /// <summary>
        /// Remove user from group
        /// </summary>
        /// <param name="groupId">ID of the group to remove from</param>
        /// <param name="userId">ID of user to be removed</param>
        /// <returns></returns>
        [HttpDelete("RemoveUserFromGroup", Name = "api_remove_user_from_group")]
        public async Task<ActionResult> RemoveUserFromGroup(
            [FromQuery] long groupId,
            [FromQuery] long userId)
        {
            try
            {
                await _groupBusiness.RemoveUserFromGroup(groupId, userId);
                return Ok(new { message = $"Removed user {userId} from group {groupId}" });
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while removing user {userId} from group {groupId}: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}