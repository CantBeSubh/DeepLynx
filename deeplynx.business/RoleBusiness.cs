using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using System.Text.Json;
using deeplynx.helpers.exceptions;

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
            roleQuery = roleQuery.Where(r => r.ProjectId == projectId.Value);
        }
        
        if (organizationId.HasValue)
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value, hideArchived);
            roleQuery = roleQuery.Where(r => r.OrganizationId == organizationId.Value);
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
    public async Task<RoleResponseDto> CreateRole(CreateRoleRequestDto dto, long? projectId, long? organizationId)
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
            ProjectId = projectId.Value,
            OrganizationId = organizationId.Value,
        };
        
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        
        // Log create Role event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = organizationId.Value,
            ProjectId = projectId.Value,
            Operation = "create",
            EntityType = "role",
            EntityId = role.Id,
            Properties = JsonSerializer.Serialize(new { role.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
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
            EntityId = role.Id,
            Properties = JsonSerializer.Serialize(new { role.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
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
            Properties = JsonSerializer.Serialize(new { role.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
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
        if (role == null || role.IsArchived)
            throw new KeyNotFoundException($"Role with id {roleId} not found or is archived");
        
        role.IsArchived = false;
        role.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        role.LastUpdatedBy = null; // TODO: add username when JWTs are implimented
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
            Properties = JsonSerializer.Serialize(new { role.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
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
            EntityId = role.Id,
            Properties = JsonSerializer.Serialize(new { role.Name }),
            LastUpdatedBy = "" // TODO: add username when JWTs are implemented
        });

        return true;
    }
}