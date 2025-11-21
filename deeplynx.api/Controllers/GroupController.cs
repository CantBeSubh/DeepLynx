using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[ApiController]
[Route("organizations/{organizationId}/groups")]
public class GroupController : ControllerBase
{
    private readonly IGroupBusiness _groupBusiness;
    private readonly ILogger<GroupController> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GroupController" /> class
    /// </summary>
    /// <param name="groupBusiness">The business logic interface for handling Group operations.</param>
    /// <param name="logger">Error/Info logging interface for database log table.</param>
    public GroupController(IGroupBusiness groupBusiness, ILogger<GroupController> logger)
    {
        _groupBusiness = groupBusiness;
        _logger = logger;
    }

    /// <summary>
    ///     List all groups within an organization
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the groups belong</param>
    /// <param name="hideArchived">Flag indicating whether to hide or show archived groups</param>
    /// <returns></returns>
    [HttpGet(Name = "api_get_all_groups")]
    public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetAllGroups(
        long organizationId,
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
    ///     Fetch Group by ID
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the group belongs</param>
    /// <param name="groupId">ID of group</param>
    /// <param name="hideArchived">Flag indicating whether to hide or show archived groups</param>
    /// <returns></returns>
    [HttpGet("{groupId}", Name = "api_get_group")]
    public async Task<ActionResult<GroupResponseDto>> GetGroup(
        long organizationId,
        long groupId,
        [FromQuery] bool hideArchived = true)
    {
        try
        {
            var group = await _groupBusiness.GetGroup(organizationId, groupId, hideArchived);
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
    ///     Get all members of a group
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the group belongs</param>
    /// <param name="groupId">ID of the group</param>
    /// <returns>List of users in the group</returns>
    [HttpGet("{groupId}/users", Name = "api_get_group_members")]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetGroupMembers(
        long organizationId,
        long groupId)
    {
        try
        {
            var members = await _groupBusiness.GetGroupMembers(organizationId, groupId);
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
    ///     Create a Group
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the group belongs</param>
    /// <param name="dto">Data structure of group to create</param>
    /// <returns></returns>
    [HttpPost(Name = "api_create_group")]
    public async Task<ActionResult<GroupResponseDto>> CreateGroup(
        long organizationId,
        [FromBody] CreateGroupRequestDto dto)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var group = await _groupBusiness.CreateGroup(currentUserId, organizationId, dto);
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
    ///     Update a Group
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the group belongs</param>
    /// <param name="groupId">ID of the group</param>
    /// <param name="dto">Fields to update</param>
    /// <returns></returns>
    [HttpPut("{groupId}", Name = "api_update_group")]
    public async Task<ActionResult<GroupResponseDto>> UpdateGroup(
        long organizationId,
        long groupId,
        [FromBody] UpdateGroupRequestDto dto)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            var group = await _groupBusiness.UpdateGroup(currentUserId, organizationId, groupId, dto);
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
    ///     Delete a group
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the group belongs</param>
    /// <param name="groupId">ID of the group to hard delete</param>
    /// <returns></returns>
    [HttpDelete("{groupId}", Name = "api_delete_group")]
    public async Task<ActionResult> DeleteGroup(
        long organizationId,
        long groupId)
    {
        try
        {
            var currentUserId = UserContextStorage.UserId;
            await _groupBusiness.DeleteGroup(currentUserId, organizationId, groupId);
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
    ///     Archive or Unarchive a Group
    /// </summary>
    /// <param name="organizationId">The ID of the organization to which the group belongs</param>
    /// <param name="groupId">The ID of the group to archive or unarchive.</param>
    /// <param name="archive">True to archive the group, false to unarchive it.</param>
    /// <returns>A message stating the group was successfully archived or unarchived.</returns>
    [HttpPatch("{groupId}", Name = "api_archive_group")]
    public async Task<IActionResult> ArchiveGroup(
        long organizationId,
        long groupId,
        [FromQuery] bool archive)
    {
        try
        {
            var userId = UserContextStorage.UserId;
            if (archive)
            {
                await _groupBusiness.ArchiveGroup(userId, organizationId, groupId);
                return Ok(new { message = $"Archived group {groupId}" });
            }

            await _groupBusiness.UnarchiveGroup(userId, organizationId, groupId);
            return Ok(new { message = $"Unarchived group {groupId}" });
        }
        catch (Exception exc)
        {
            var action = archive ? "archiving" : "unarchiving";
            var message = $"An error occurred while {action} group {groupId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    ///     Add user to group
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the group belongs</param>
    /// <param name="groupId">ID of the group</param>
    /// <param name="userId">ID of the user to be added</param>
    /// <returns></returns>
    [HttpPost("{groupId}/users", Name = "api_add_user_to_group")]
    public async Task<ActionResult> AddUserToGroup(
        long organizationId,
        long groupId,
        [FromQuery] long userId)
    {
        try
        {
            await _groupBusiness.AddUserToGroup(organizationId, groupId, userId);
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
    ///     Remove user from group
    /// </summary>
    /// <param name="organizationId">ID of the organization to which the group belongs</param>
    /// <param name="groupId">ID of the group to remove from</param>
    /// <param name="userId">ID of user to be removed</param>
    /// <returns></returns>
    [HttpDelete("{groupId}/users/{userId}", Name = "api_remove_user_from_group")]
    public async Task<ActionResult> RemoveUserFromGroup(
        long organizationId,
        long groupId,
        long userId)
    {
        try
        {
            await _groupBusiness.RemoveUserFromGroup(organizationId, groupId, userId);
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