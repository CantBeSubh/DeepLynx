using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using System.Text.Json;

namespace deeplynx.business;

/// <summary>
/// PermissionBusiness is unique from other business classes in the sense that it
/// is partially protected. Default permissions (marked with "isDefault")
/// should not be tampered with via standard CRUD operations via the API.
/// As such, special checks are in place to ensure that
/// permissions being edited by the user are only those which were originally
/// user-defined.
/// </summary>
public class PermissionBusiness : IPermissionBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;
    private readonly ICacheBusiness _cacheBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for permission operations</param>
    /// <param name="eventBusiness">Used to access cache operations</param>
    /// <param name="cacheBusiness">Used for logging events during CRUD operations</param>
    public PermissionBusiness(DeeplynxContext context, IEventBusiness eventBusiness, ICacheBusiness cacheBusiness)
    {
        _context = context;
        _eventBusiness = eventBusiness;
        _cacheBusiness = cacheBusiness;
    }

    /// <summary>
    /// List all permissions
    /// </summary>
    /// <param name="labelId">(Optional)ID of a sensitivity label to filter by</param>
    /// <param name="projectId">(Optional)ID of a project to filter by</param>
    /// <param name="organizationId">(Optional)ID of an organization to filter by</param>
    /// <param name="hideArchived">Flag indicating whether to search on archived permissions</param>
    /// <returns>A list of permissions</returns>
    public async Task<IEnumerable<PermissionResponseDto>> GetAllPermissions(
        long? labelId, long? projectId, long? organizationId,
        bool hideArchived = true)
    {
        var permissionQuery = _context.Permissions.Where(p =>
            p.IsDefault || (!p.IsDefault &&         // ensure Default perms are returned regardless of filters
                (!labelId.HasValue || p.LabelId == labelId) &&                        // check for label filter
                (!projectId.HasValue || p.ProjectId == projectId) &&                  // check for project filter
                (!organizationId.HasValue || p.OrganizationId == organizationId)));   // check for org filter

        if (hideArchived)
            permissionQuery = permissionQuery.Where(p => !p.IsArchived);

        return await permissionQuery.Select(p => new PermissionResponseDto()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Action = p.Action,
            Resource = p.Resource,
            LastUpdatedAt = p.LastUpdatedAt,
            LastUpdatedBy = p.LastUpdatedBy,
            IsArchived = p.IsArchived,
            LabelId = p.LabelId,
            ProjectId = p.ProjectId,
            OrganizationId = p.OrganizationId,
            IsDefault = p.IsDefault,
        })
        .ToListAsync();
    }

    /// <summary>
    /// Get a permission by ID
    /// </summary>
    /// <param name="permissionId">ID of the permission to retrieve</param>
    /// <param name="hideArchived"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<PermissionResponseDto> GetPermission(long permissionId, bool hideArchived = true)
    {
        var permission = await _context.Permissions
            .Where(p => p.Id == permissionId)
            .FirstOrDefaultAsync();

        if (permission == null)
            throw new KeyNotFoundException($"Permission with id {permissionId} not found");

        if (hideArchived && permission.IsArchived)
            throw new KeyNotFoundException($"Permission with id {permissionId} is archived");

        return new PermissionResponseDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Action = permission.Action,
            Resource = permission.Resource,
            LastUpdatedAt = permission.LastUpdatedAt,
            LastUpdatedBy = permission.LastUpdatedBy,
            IsArchived = permission.IsArchived,
            LabelId = permission.LabelId,
            ProjectId = permission.ProjectId,
            OrganizationId = permission.OrganizationId,
            IsDefault = permission.IsDefault
        };
    }

    /// <summary>
    /// Create a new user-defined permission
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="dto">The permission to be created</param>
    /// <param name="projectId">ID of the project to which the permission belongs</param>
    /// <param name="organizationId">ID of the organization to which the permission belongs</param>
    /// <returns>The newly created permission</returns>
    /// <exception cref="ArgumentException">Returned if project/org both supplied or no project/org supplied</exception>
    public async Task<PermissionResponseDto> CreatePermission(
        long currentUserId,
        CreatePermissionRequestDto dto,
        long? projectId, long? organizationId)
    {
        // ensure one and only one of projectID or organizationID is supplied
        if (!projectId.HasValue && !organizationId.HasValue)
            throw new ArgumentException("One of Project ID or Organization ID must be provided");
        if (projectId.HasValue && organizationId.HasValue)
            throw new ArgumentException("Please provide only one of Project ID or Organization ID, not both");

        if (projectId.HasValue)
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
        if (organizationId.HasValue)
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);

        // Note that the CreatePermission dto only allows for the creation of permissions
        // using labelId. Any Default permissions such as "write projects" should not
        // be manipulated by users.
        ValidationHelper.ValidateModel(dto);
        var permission = new Permission
        {
            Name = dto.Name,
            Description = dto.Description,
            Action = dto.Action,
            Resource = dto.Resource,
            LabelId = dto.LabelId,
            IsDefault = false,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = currentUserId,
            ProjectId = projectId,
            OrganizationId = organizationId,
        };

        _context.Permissions.Add(permission);
        
        // Log create Permission event
        var eventLog = new CreateEventRequestDto
        {
            Operation = "create",
            EntityType = "permission",
            EntityId = permission.Id,
            EntityName = permission.Name,
            Properties = JsonSerializer.Serialize(new { permission.Name }),
        };
        
        // determine if this is project level or organization level
        if (permission.ProjectId.HasValue)
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, null, permission.ProjectId);
        }
        else
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, permission.OrganizationId, null);
        }
        
        await _context.SaveChangesAsync();
        
        return new PermissionResponseDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Action = permission.Action,
            Resource = permission.Resource,
            LastUpdatedAt = permission.LastUpdatedAt,
            LastUpdatedBy = permission.LastUpdatedBy,
            IsArchived = permission.IsArchived,
            LabelId = permission.LabelId,
            ProjectId = permission.ProjectId,
            OrganizationId = permission.OrganizationId,
            IsDefault = permission.IsDefault
        };
    }

    /// <summary>
    /// Update an existing user-defined permission
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="permissionId">ID of the permission to be updated</param>
    /// <param name="dto">New information on the permission</param>
    /// <returns>The newly updated permission</returns>
    /// <exception cref="KeyNotFoundException">Returned if the permission is not found or is uneditable</exception>
    public async Task<PermissionResponseDto> UpdatePermission(long currentUserId, long permissionId, UpdatePermissionRequestDto dto)
    {
        var permission = await _context.Permissions.FindAsync(permissionId);
        // ensure that default permissions cannot be edited
        if (permission == null || permission.IsArchived)
            throw new KeyNotFoundException($"Permission with id {permissionId} not found");
        if (permission.IsDefault)
            throw new KeyNotFoundException($"Permission with id {permissionId} cannot be updated");

        permission.Name = dto.Name ?? permission.Name;
        permission.Description = dto.Description ?? permission.Description;
        permission.LabelId = dto.LabelId ?? permission.LabelId;
        permission.Action = dto.Action ?? permission.Action;
        permission.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        permission.LastUpdatedBy = currentUserId;

        _context.Permissions.Update(permission);

        // Log update Permission event
        var eventLog = new CreateEventRequestDto
        {
            Operation = "update",
            EntityType = "permission",
            EntityId = permission.Id,
            EntityName = permission.Name,
            Properties = JsonSerializer.Serialize(new { permission.Name }),
        };
        
        // determine if this is project level or organization level
        if (permission.ProjectId.HasValue)
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, null, permission.ProjectId);
        }
        else
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, permission.OrganizationId, null);
        }
        
        await _context.SaveChangesAsync();
        
        return new PermissionResponseDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Action = permission.Action,
            Resource = permission.Resource,
            LastUpdatedAt = permission.LastUpdatedAt,
            LastUpdatedBy = permission.LastUpdatedBy,
            IsArchived = permission.IsArchived,
            LabelId = permission.LabelId,
            ProjectId = permission.ProjectId,
            OrganizationId = permission.OrganizationId,
            IsDefault = permission.IsDefault
        };
    }

    /// <summary>
    /// Archive a permission
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="permissionId">The ID of the permission to be archived</param>
    /// <returns>Boolean true upon success</returns>
    /// <exception cref="KeyNotFoundException">Returned if the permission is not found or is uneditable</exception>
    public async Task<bool> ArchivePermission(long currentUserId, long permissionId)
    {
        var permission = await _context.Permissions.FindAsync(permissionId);
        // ensure that default permissions cannot be edited
        if (permission == null || permission.IsArchived)
            throw new KeyNotFoundException($"Permission with id {permissionId} not found or is already archived");
        if (permission.IsDefault)
            throw new KeyNotFoundException($"Permission with id {permissionId} cannot be updated");

        permission.IsArchived = true;
        permission.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        permission.LastUpdatedBy = currentUserId;
        _context.Permissions.Update(permission);

        // Log archive Permission event
        var eventLog = new CreateEventRequestDto
        {
            Operation = "archive",
            EntityType = "permission",
            EntityId = permission.Id,
            EntityName = permission.Name,
            Properties = JsonSerializer.Serialize(new { permission.Name }),
        };
        
        // determine if this is project level or organization level
        if (permission.ProjectId.HasValue)
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, null, permission.ProjectId);
        }
        else
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, permission.OrganizationId, null);
        }
        
        await _context.SaveChangesAsync();
        
        return true;
    }

    /// <summary>
    /// Unarchive a permission
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="permissionId">The ID of the permission to be unarchived</param>
    /// <returns>Boolean true upon success</returns>
    /// <exception cref="KeyNotFoundException">Returned if the permission is not found or is uneditable</exception>
    public async Task<bool> UnarchivePermission(long currentUserId, long permissionId)
    {
        var permission = await _context.Permissions.FindAsync(permissionId);
        // ensure that default permissions cannot be edited
        if (permission != null && permission.IsDefault)
            throw new KeyNotFoundException($"Permission with id {permissionId} cannot be updated");
        if (permission == null || !permission.IsArchived)
            throw new KeyNotFoundException($"Permission with id {permissionId} not found or is not archived");

        permission.IsArchived = false;
        permission.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        permission.LastUpdatedBy = currentUserId;
        _context.Permissions.Update(permission);

        // Log unarchive Permission event
        var eventLog = new CreateEventRequestDto
        {
            Operation = "unarchive",
            EntityType = "permission",
            EntityId = permission.Id,
            EntityName = permission.Name,
            Properties = JsonSerializer.Serialize(new { permission.Name }),
        };
        
        // determine if this is project level or organization level
        if (permission.ProjectId.HasValue)
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, null, permission.ProjectId);
        }
        else
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, permission.OrganizationId, null);
        }
        
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Delete a permission
    /// </summary>
    /// <param name="permissionId">The ID of the permission to be deleted</param>
    /// <returns>Boolean true upon success</returns>
    /// <exception cref="KeyNotFoundException">Returned if the permission is not found or is uneditable</exception>
    public async Task<bool> DeletePermission( long currentUserId, long permissionId)
    {
        var permission = await _context.Permissions.FindAsync(permissionId);
        // ensure that default permissions cannot be edited
        if (permission == null || permission.IsArchived)
            throw new KeyNotFoundException($"Permission with id {permissionId} not found");
        if (permission.IsDefault)
            throw new KeyNotFoundException($"Permission with id {permissionId} cannot be deleted");

        _context.Permissions.Remove(permission);

        // Log delete Permission event
        var eventLog = new CreateEventRequestDto
        {
            Operation = "delete",
            EntityType = "permission",
            EntityId = permission.Id,
            EntityName = permission.Name,
            Properties = JsonSerializer.Serialize(new { permission.Name }),
        };
        
        // determine if this is project level or organization level
        if (permission.ProjectId.HasValue)
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, null, permission.ProjectId);
        }
        else
        {
            await _eventBusiness.CreateEvent(currentUserId, eventLog, permission.OrganizationId, null);
        }
        
        await _context.SaveChangesAsync();

        return true;
    }
}