using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using System.Text.Json;

namespace deeplynx.business;

public class GroupBusiness : IGroupBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupBusiness"/> class.
    /// </summary>
    /// <param name="context">Database context used for group CRUD operations</param>
    /// <param name="eventBusiness">Used for logging events during CRUD operations</param>
    public GroupBusiness(DeeplynxContext context, IEventBusiness eventBusiness)
    {
        _context = context;
        _eventBusiness = eventBusiness;
    }

    /// <summary>
    /// Get all groups within an organization
    /// </summary>
    /// <param name="organizationId">ID of the organization from which to list groups</param>
    /// <param name="hideArchived">Boolean indicating whether to hide archived groups from results</param>
    /// <returns>An array of groups within the given organization</returns>
    public async Task<IEnumerable<GroupResponseDto>> GetAllGroups(long organizationId, bool hideArchived = true)
    {
        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);

        var groupQuery = _context.Groups.Where(g => g.OrganizationId == organizationId);
        
        if (hideArchived)
        {
            groupQuery = groupQuery.Where(g => !g.IsArchived);
        }

        return groupQuery
            .Select(g => new GroupResponseDto()
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                LastUpdatedAt = g.LastUpdatedAt,
                LastUpdatedBy = g.LastUpdatedBy,
                IsArchived = g.IsArchived,
                OrganizationId = g.OrganizationId,
            });
    }

    /// <summary>
    /// Retrieves a specific group by ID
    /// </summary>
    /// <param name="groupId">The ID by which to retrieve the group</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived groups from the result</param>
    /// <returns>The given group to return</returns>
    /// <exception cref="KeyNotFoundException">Returned if the group is not found or is archived</exception>
    public async Task<GroupResponseDto> GetGroup(long groupId, bool hideArchived = true)
    {
        var group = await _context.Groups
            .Where(g => g.Id == groupId)
            .FirstOrDefaultAsync();
        
        if (group == null)
            throw new KeyNotFoundException($"Group with id {groupId} does not exist");
        
        if (hideArchived && group.IsArchived)
            throw new KeyNotFoundException($"Group with id {groupId} is archived");

        return new GroupResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            LastUpdatedAt = group.LastUpdatedAt,
            LastUpdatedBy = group.LastUpdatedBy,
            IsArchived = group.IsArchived,
            OrganizationId = group.OrganizationId,
        };
    }

    /// <summary>
    /// Create a group
    /// </summary>
    /// <param name="organizationId">The organization ID to which the group will belong</param>
    /// <param name="dto">The data from the user on how group should be configured</param>
    /// <returns>The newly created group</returns>
    public async Task<GroupResponseDto> CreateGroup(long organizationId, CreateGroupRequestDto dto)
    {
        await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId);
        ValidationHelper.ValidateModel(dto);
        var group = new Group
        {
            Name = dto.Name,
            Description = dto.Description,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null // TODO: implement user ID here when JWT tokens are ready
        };
        
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        
        // Log create Group event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = organizationId,
            Operation = "create",
            EntityType = "group",
            EntityId = group.Id,
            Properties = JsonSerializer.Serialize(new { group.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
        });

        return new GroupResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            LastUpdatedAt = group.LastUpdatedAt,
            LastUpdatedBy = group.LastUpdatedBy,
            IsArchived = group.IsArchived,
            OrganizationId = group.OrganizationId,
        };
    }

    /// <summary>
    /// Update a group with new information
    /// </summary>
    /// <param name="groupId">The ID of the group to be updated</param>
    /// <param name="dto">The data transfer object holding information </param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<GroupResponseDto> UpdateGroup(long groupId, UpdateGroupRequestDto dto)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.IsArchived)
            throw new KeyNotFoundException($"Group with id {groupId} not found");
        
        group.Name = dto.Name ?? group.Name;
        group.Description = dto.Description ?? group.Description;
        group.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        group.LastUpdatedBy = null; // TODO: handled in the future by JWT
        
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
        
        // Log update Group event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = group.OrganizationId,
            Operation = "update",
            EntityType = "group",
            EntityId = group.Id,
            Properties = JsonSerializer.Serialize(new { group.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
        });

        return new GroupResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            LastUpdatedAt = group.LastUpdatedAt,
            LastUpdatedBy = group.LastUpdatedBy,
            IsArchived = group.IsArchived,
            OrganizationId = group.OrganizationId,
        };
    }

    /// <summary>
    /// Archive a specific group by ID
    /// </summary>
    /// <param name="groupId">ID of the group to archive</param>
    /// <returns>Boolean true on successful archive</returns>
    /// <exception cref="KeyNotFoundException">Returned if group not found</exception>
    public async Task<bool> ArchiveGroup(long groupId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.IsArchived)
            throw new KeyNotFoundException($"Group with id {groupId} not found or is archived");
        
        group.IsArchived = true;
        group.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        group.LastUpdatedBy = null; // TODO: add username when JWTs are implemented
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
        
        // Log archive Group event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = group.OrganizationId,
            Operation = "archive",
            EntityType = "group",
            EntityId = group.Id,
            Properties = JsonSerializer.Serialize(new { group.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
        });

        return true;
    }
    
    /// <summary>
    /// Unarchive a specific group by ID
    /// </summary>
    /// <param name="groupId">ID of the group to unarchive</param>
    /// <returns>Boolean true on successful unarchive</returns>
    /// <exception cref="KeyNotFoundException">Returned if group not found</exception>
    public async Task<bool> UnarchiveGroup(long groupId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || !group.IsArchived)
            throw new KeyNotFoundException($"Group with id {groupId} not found or is not archived");
        
        group.IsArchived = false;
        group.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        group.LastUpdatedBy = null; // TODO: add username when JWTs are implemented
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
        
        // Log unarchive Group event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = group.OrganizationId,
            Operation = "archive",
            EntityType = "group",
            EntityId = group.Id,
            Properties = JsonSerializer.Serialize(new { group.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
        });

        return true;
    }
    
    /// <summary>
    /// Delete a specific group by ID
    /// </summary>
    /// <param name="groupId">ID of the group to delete</param>
    /// <returns>Boolean true on successful delete</returns>
    /// <exception cref="KeyNotFoundException">Returned if group not found</exception>
    public async Task<bool> DeleteGroup(long groupId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || !group.IsArchived)
            throw new KeyNotFoundException($"Group with id {groupId} not found");
        
        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        
        // Log delete Group event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = group.OrganizationId,
            Operation = "delete",
            EntityType = "group",
            EntityId = group.Id,
            Properties = JsonSerializer.Serialize(new { group.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
        });

        return true;
    }

    /// <summary>
    /// Add a user to a group
    /// </summary>
    /// <param name="groupId">ID of group to add user to</param>
    /// <param name="userId">ID of user to add to group</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Returned if group or user not found</exception>
    public async Task<bool> AddUserToGroup(long groupId, long userId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || !group.IsArchived)
            throw new KeyNotFoundException($"Group with id {groupId} not found");
        
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null || !user.IsArchived)
            throw new KeyNotFoundException($"User with id {userId} does not exist");
        
        group.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Remove a user from a group
    /// </summary>
    /// <param name="groupId">ID of group to remove user from</param>
    /// <param name="userId">ID of user to remove from group</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Returned if group or user not found</exception>
    public async Task<bool> RemoveUserFromGroup(long groupId, long userId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || !group.IsArchived)
            throw new KeyNotFoundException($"Group with id {groupId} not found");
        
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null || !user.IsArchived)
            throw new KeyNotFoundException($"User with id {userId} does not exist");
        
        group.Users.Remove(user);
        await _context.SaveChangesAsync();
        
        return true;
    }
}