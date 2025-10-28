using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using System.Text.Json;
using deeplynx.helpers.exceptions;
using Npgsql;

namespace deeplynx.business;

public class RoleBusiness : IRoleBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;
    private readonly ICacheBusiness _cacheBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for role operations</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    /// <param name="eventBusiness">Used for logging events during CRUD operations</param>
    public RoleBusiness(DeeplynxContext context, ICacheBusiness cacheBusiness, IEventBusiness eventBusiness)
    {
        _context = context;
        _eventBusiness = eventBusiness;
        _cacheBusiness = cacheBusiness;
    }

    /// <summary>
    /// Get all roles for a given project and/or organization
    /// </summary>
    /// <param name="projectId">ID of the project across which to search</param>
    /// <param name="organizationId">ID of the organization across which to search</param>
    /// <param name="hideArchived">Flag indicating whether to search on archived roles</param>
    /// <returns>A list of roles</returns>
    public async Task<IEnumerable<RoleResponseDto>> GetAllRoles(
        long? projectId, long? organizationId, bool hideArchived = true)
    {
        var roleQuery = _context.Roles.AsQueryable();

        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness, hideArchived);
            roleQuery = roleQuery.Where(r => r.ProjectId == projectId);
        }

        if (organizationId.HasValue)
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value, hideArchived);
            roleQuery = roleQuery.Where(r => r.OrganizationId == organizationId);
        }

        if (hideArchived)
        {
            roleQuery = roleQuery.Where(r => !r.IsArchived);
        }

        return await roleQuery.Select(r => new RoleResponseDto()
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                LastUpdatedAt = r.LastUpdatedAt,
                LastUpdatedBy = r.LastUpdatedBy,
                IsArchived = r.IsArchived,
                ProjectId = r.ProjectId,
                OrganizationId = r.OrganizationId,
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get a role by ID
    /// </summary>
    /// <param name="roleId">ID of the role to retrieve</param>
    /// <param name="hideArchived">Flag indicating whether to search archived roles</param>
    /// <returns>The requested role</returns>
    /// <exception cref="KeyNotFoundException">Thrown if role not found</exception>
    public async Task<RoleResponseDto> GetRole(long roleId, bool hideArchived = true)
    {
        var role = await _context.Roles
            .Where(r => r.Id == roleId)
            .FirstOrDefaultAsync();

        if (role == null)
            throw new KeyNotFoundException($"Role with id {roleId} not found");

        if (hideArchived && role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} is archived");

        return new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            LastUpdatedAt = role.LastUpdatedAt,
            LastUpdatedBy = role.LastUpdatedBy,
            IsArchived = role.IsArchived,
            ProjectId = role.ProjectId,
            OrganizationId = role.OrganizationId,
        };
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <param name="dto">Data Transfer Object containing new role information</param>
    /// <param name="projectId">ID of the project to which the role belongs</param>
    /// <param name="organizationId">ID of the organization to which the role belongs</param>
    /// <returns>The newly created role</returns>
    /// <exception cref="ArgumentException">Returned if project/org both supplied or no project/org supplied</exception>
    public async Task<RoleResponseDto> CreateRole(
        CreateRoleRequestDto dto, long? projectId = null, long? organizationId = null)
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

        ValidationHelper.ValidateModel(dto);
        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null, // TODO: implement user ID here when JWT tokens are ready,
            ProjectId = projectId,
            OrganizationId = organizationId,
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Log create Role event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = organizationId,
            ProjectId = projectId,
            Operation = "create",
            EntityType = "role",
            EntityId = role.Id,
            EntityName = role.Name,
            Properties = JsonSerializer.Serialize(new { role.Name }),
        });

        return new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            LastUpdatedAt = role.LastUpdatedAt,
            LastUpdatedBy = role.LastUpdatedBy,
            IsArchived = role.IsArchived,
            ProjectId = role.ProjectId,
            OrganizationId = role.OrganizationId,
        };
    }

    /// <summary>
    /// Upsert multiple roles at a time
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    public async Task<List<RoleResponseDto>> BulkCreateRoles(long projectId, List<CreateRoleRequestDto> roles)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);

        // TODO: add support for organization roles
        // Bulk insert into roles; if there is a name collision, update the description if present
        var sql = @"
            INSERT INTO deeplynx.roles (project_id, name, description, last_updated_at, last_updated_by)
            VALUES {0}
            ON CONFLICT (project_id, name) DO UPDATE SET
                description = COALESCE(EXCLUDED.description, roles.description),
                last_updated_at = @now
            RETURNING id, project_id, organization_id, name, description, last_updated_at, is_archived, last_updated_by;
        ";

        // establish "constant" parameters
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@projectId", projectId),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
        };

        // establish "dynamic" parameters (new for each dto in the list)
        parameters.AddRange(roles.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@p{i}_name", dto.Name),
            new NpgsqlParameter($"@p{i}_desc", (object?)dto.Description ?? DBNull.Value),
        }));

        // stringify the params and comma separate them
        var valueTuples = string.Join(", ", roles.Select((dto, i) =>
            $"(@projectId, @p{i}_name, @p{i}_desc, @now, NULL)"));

        // put everything together and execute the query
        sql = string.Format(sql, valueTuples);

        // returns the resulting upserted classes
        var result = await _context.Database
            .SqlQueryRaw<RoleResponseDto>(sql, parameters.ToArray())
            .ToListAsync();

        // for each created class Bulk log events
        var events = new List<CreateEventRequestDto> { };
        foreach (var item in result)
        {
            events.Add(new CreateEventRequestDto
            {
                ProjectId = projectId,
                Operation = "create",
                EntityType = "role",
                EntityId = item.Id,
                EntityName = item.Name,
                DataSourceId = null,
                Properties = JsonSerializer.Serialize(new {item.Name}),
            });
        }
        await _eventBusiness.BulkCreateEvents(projectId, events);

        return result;
    }

    /// <summary>
    /// Update role information
    /// </summary>
    /// <param name="roleId">ID of the role to be updated</param>
    /// <param name="dto">Data Transfer Object containing new role information</param>
    /// <returns>The newly updated role</returns>
    /// <exception cref="KeyNotFoundException">Returned if role not found</exception>
    public async Task<RoleResponseDto> UpdateRole(long roleId, UpdateRoleRequestDto dto)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null || role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found");

        role.Name = dto.Name ?? role.Name;
        role.Description = dto.Description ?? role.Description;
        role.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        role.LastUpdatedBy = null;  // TODO: implement user ID here when JWT tokens are ready

        _context.Roles.Update(role);
        await _context.SaveChangesAsync();

        // Log update Role event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = role.OrganizationId,
            ProjectId = role.ProjectId,
            Operation = "update",
            EntityType = "role",
            EntityName = role.Name,
            EntityId = role.Id,
            Properties = JsonSerializer.Serialize(new { role.Name }),
        });

        return new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            LastUpdatedAt = role.LastUpdatedAt,
            LastUpdatedBy = role.LastUpdatedBy,
            IsArchived = role.IsArchived,
            ProjectId = role.ProjectId,
            OrganizationId = role.OrganizationId,
        };
    }

    /// <summary>
    /// Archive a role by ID. Remove role from downstream project members
    /// </summary>
    /// <param name="roleId">ID of role to archive</param>
    /// <returns>Boolean true if executed successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if role not found or is already archived</exception>
    /// <exception cref="DependencyDeletionException">Returned if role removal from project members fails</exception>
    public async Task<bool> ArchiveRole(long roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null || role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found or is archived");

        // set lastUpdatedAt timestamp
        var lastUpdatedAt = DateTime.UtcNow;

        // run archive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the archive role procedure, which archives this role and
                // removes this role from anyone holding it in any projects
                var archived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.archive_role({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)",
                    roleId, lastUpdatedAt
                );

                if (archived == 0) // if 0 records were updated, assume a failure
                    throw new DependencyDeletionException(
                        $"Unable to archive role {roleId} or its downstream dependents.");

                await transaction.CommitAsync();
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException(
                    $"Unable to archive role {roleId} or its downstream dependents: {exc}");
            }
        }

        // Log archive Role event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = role.OrganizationId,
            ProjectId = role.ProjectId,
            Operation = "archive",
            EntityType = "role",
            EntityId = role.Id,
            EntityName = role.Name,
            Properties = JsonSerializer.Serialize(new { role.Name }),
        });

        return true;
    }

    /// <summary>
    /// Unarchive a role by ID
    /// </summary>
    /// <param name="roleId">ID of role to unarchive</param>
    /// <returns>Boolean true if executed successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if role not found or is not archived</exception>
    public async Task<bool> UnarchiveRole(long roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null || !role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found or is not archived");

        role.IsArchived = false;
        role.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        role.LastUpdatedBy = null; // TODO: add username when JWTs are implemented
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();

        // Log unarchive Role event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = role.OrganizationId,
            ProjectId = role.ProjectId,
            Operation = "unarchive",
            EntityType = "role",
            EntityId = role.Id,
            EntityName = role.Name,
            Properties = JsonSerializer.Serialize(new { role.Name }),
        });

        return true;
    }

    /// <summary>
    /// Delete a role by ID
    /// </summary>
    /// <param name="roleId">ID of role to delete</param>
    /// <returns>Boolean true if executed successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if role not found</exception>
    public async Task<bool> DeleteRole(long roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null || role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found or is archived");

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        // Log archive Role event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = role.OrganizationId,
            ProjectId = role.ProjectId,
            Operation = "delete",
            EntityType = "role",
            EntityName = role.Name,
            EntityId = role.Id,
            Properties = JsonSerializer.Serialize(new { role.Name }),
        });

        return true;
    }

    /// <summary>
    /// List all permissions for a given role
    /// </summary>
    /// <param name="roleId">ID of the role across which to search permissions</param>
    /// <param name="hideArchived">Flag indicating whether to search on archived permissions</param>
    /// <returns>A list of permissions</returns>
    /// <exception cref="KeyNotFoundException">Returned if role not found</exception>
    public async Task<IEnumerable<PermissionResponseDto>> GetPermissionsByRole(long roleId)
    {
        // check if role exists
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null || role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found");

        return role.Permissions.Select(p => new PermissionResponseDto()
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
            IsDefault = p.IsDefault
        });
    }

    /// <summary>
    /// Add a permission to a role
    /// </summary>
    /// <param name="roleId">ID of the role to add permission to</param>
    /// <param name="permissionId">ID of the permission to add</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Returned if role or permission not found</exception>
    /// <exception cref="InvalidOperationException">Returned if permission already exists for role</exception>
    public async Task<bool> AddPermissionToRole(long roleId, long permissionId)
    {
        // check if role exists
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null || role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found");

        // check if permission exists
        var permission = await _context.Permissions.FindAsync(permissionId);
        if (permission == null || permission.IsArchived)
            throw new KeyNotFoundException($"Permission with id {permissionId} not found");

        // check if permission is already assigned to the role
        if (role.Permissions.Any(p => p.Id == permission.Id))
            throw new ArgumentException($"Permission with id {permissionId} already exists as part of role {roleId}");

        role.Permissions.Add(permission);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Remove a permission from a role
    /// </summary>
    /// <param name="roleId">ID of the role to remove permission from</param>
    /// <param name="permissionId">ID of the permission to remove</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Returned if role not found or permission not assigned to role</exception>
    public async Task<bool> RemovePermissionFromRole(long roleId, long permissionId)
    {
        // check if role exists
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null || role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found");

        // check if permission exists on role
        var permission = role.Permissions.FirstOrDefault(p => p.Id == permissionId);
        if (permission == null || permission.IsArchived)
            throw new KeyNotFoundException($"Permission with id {permissionId} is not assigned to role {roleId}");

        role.Permissions.Remove(permission);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Set all permissions for a role (replaces existing permissions)
    /// </summary>
    /// <param name="roleId">ID of the role to update permissions for</param>
    /// <param name="permissionIds">Array of permission IDs to assign to the role</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Returned if role not found or any permission ID is invalid</exception>
    public async Task<bool> SetPermissionsForRole(long roleId, long[] permissionIds)
    {
        // check if role exists
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null || role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found");

        // validate that all permissions IDs exist
        var permissions = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync();

        if (permissions.Count != permissionIds.Length)
        {
            var missingIds = permissionIds.Except(permissions.Select(p => p.Id).ToList());
            throw new KeyNotFoundException($"Permissions not found: {string.Join(", ", missingIds)}");
        }

        // clear existing permissions and add new ones
        role.Permissions.Clear();
        foreach (var permission in permissions)
            role.Permissions.Add(permission);

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Set all permissions for a role by pattern
    /// </summary>
    /// <param name="roleId">ID of the role to update permissions for</param>
    /// <param name="permissionPatterns">Dictionary of resource: action[] permission patterns</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Returned if role not found</exception>
    public async Task<bool> SetPermissionsByPattern(long roleId, Dictionary<string, string[]> permissionPatterns)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null || role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found");

        // get the list of resources we're interested in
        var resources = permissionPatterns.Keys.ToList();

        // fetch all permissions for these resources
        var allPermissions = await _context.Permissions
            .Where(p => resources.Contains(p.Resource))
            .ToListAsync();

        // filter in memory to match the exact actions
        var matchingPermissions = allPermissions
            .Where(p => permissionPatterns.ContainsKey(p.Resource) &&
                        permissionPatterns[p.Resource].Contains(p.Action))
            .ToList();

        // clear existing permissions and add new ones
        role.Permissions.Clear();
        foreach (var permission in matchingPermissions)
            role.Permissions.Add(permission);

        await _context.SaveChangesAsync();
        return true;
    }
}