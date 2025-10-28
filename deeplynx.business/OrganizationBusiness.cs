using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace deeplynx.business;

public class OrganizationBusiness : IOrganizationBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ILogger<OrganizationBusiness> _logger;
    private readonly IEventBusiness _eventBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for organization CRUD operations.</param>
    /// <param name="eventBusiness">Used for logging events during CRUD operations.</param>
    /// <param name="logger"></param>
    public OrganizationBusiness(
        DeeplynxContext context,
        IEventBusiness eventBusiness,
        ILogger<OrganizationBusiness> logger
    )
    {
        _context = context;
        _eventBusiness = eventBusiness;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all organizations
    /// </summary>
    /// <param name="hideArchived">Flag indicating whether to hide archived organizations from the result</param>
    /// <returns>A list of organizations</returns>
    public async Task<IEnumerable<OrganizationResponseDto>> GetAllOrganizations(bool hideArchived = true)
    {
        var organizationQuery = _context.Organizations.AsQueryable();

        if (hideArchived)
        {
            organizationQuery = organizationQuery.Where(o => !o.IsArchived);
        }

        var organizations = await organizationQuery.ToListAsync();

        return organizations
            .Select(o => new OrganizationResponseDto()
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
                LastUpdatedAt = o.LastUpdatedAt,
                LastUpdatedBy = o.LastUpdatedBy,
                IsArchived = o.IsArchived,
            });
    }

    /// <summary>
    /// Retrieves a specific organization by ID
    /// </summary>
    /// <param name="organizationId">The ID by which to retrieve the organization</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived organizations from the result</param>
    /// <returns>The given organization to return</returns>
    /// <exception cref="KeyNotFoundException">Returned if the organization is not found or is archived</exception>
    public async Task<OrganizationResponseDto> GetOrganization(long organizationId, bool hideArchived = true)
    {
        var organization = await _context.Organizations
            .Where(o => o.Id == organizationId)
            .FirstOrDefaultAsync();

        if (organization == null)
        {
            throw new KeyNotFoundException($"Organization with id {organizationId} does not exist");
        }

        if (hideArchived && organization.IsArchived)
        {
            throw new KeyNotFoundException($"Organization with id {organizationId} is archived");
        }

        return new OrganizationResponseDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            LastUpdatedAt = organization.LastUpdatedAt,
            LastUpdatedBy = organization.LastUpdatedBy,
            IsArchived = organization.IsArchived,
        };
    }

    /// <summary>
    /// Creates a new organization and logs the creation event.
    /// </summary>
    /// <param name="dto">A data transfer object with details on the organization to be created.</param>
    /// <returns>The created organization.</returns>
    public async Task<OrganizationResponseDto> CreateOrganization(CreateOrganizationRequestDto dto)
    {
        ValidationHelper.ValidateModel(dto);
        var organization = new Organization
        {
            Name = dto.Name,
            Description = dto.Description,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null // TODO: Implement user ID here when JWT tokens are ready
        };

        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        // Log create Organization event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = organization.Id,
            Operation = "create",
            EntityType = "organization",
            EntityId = organization.Id,
            EntityName = organization.Name,
            Properties = JsonSerializer.Serialize(new { organization.Name }),
        });

        return new OrganizationResponseDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            LastUpdatedAt = organization.LastUpdatedAt,
            LastUpdatedBy = organization.LastUpdatedBy,
            IsArchived = organization.IsArchived,
        };
    }

    /// <summary>
    /// Update an organization by ID
    /// </summary>
    /// <param name="organizationId">The ID of the organization to be updated</param>
    /// <param name="dto">A data transfer object with details on the organization to be updated</param>
    /// <returns>The updated organization</returns>
    /// <exception cref="KeyNotFoundException">Returned if organization to update was not found</exception>
    public async Task<OrganizationResponseDto> UpdateOrganization(long organizationId, UpdateOrganizationRequestDto dto)
    {
        var organization = await _context.Organizations.FindAsync(organizationId);

        if (organization == null || organization.IsArchived)
        {
            throw new KeyNotFoundException($"Organization with id {organizationId} does not exist");
        }

        organization.Name = dto.Name ?? organization.Name;
        organization.Description = dto.Description ?? organization.Description;
        organization.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        organization.LastUpdatedBy = null; // TODO: handled in the future by JWT

        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync();

        // log update Organization event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = organization.Id,
            Operation = "update",
            EntityType = "organization",
            EntityId = organization.Id,
            EntityName = organization.Name,
            Properties = JsonSerializer.Serialize(new { organization.Name }),
        });

        return new OrganizationResponseDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            LastUpdatedAt = organization.LastUpdatedAt,
            LastUpdatedBy = organization.LastUpdatedBy,
            IsArchived = organization.IsArchived,
        };
    }

    /// <summary>
    /// Archive a specific organization by ID
    /// </summary>
    /// <param name="organizationId">The ID of the organization to archive</param>
    /// <returns>Boolean true on successful archive</returns>
    /// <exception cref="KeyNotFoundException">Returned if organization not found</exception>
    public async Task<bool> ArchiveOrganization(long organizationId)
    {
        var organization = await _context.Organizations.FindAsync(organizationId);

        if (organization == null || organization.IsArchived)
            throw new KeyNotFoundException($"Organization with id {organizationId} not found");

        // TODO: determine if this needs to be a cascade archive instead
        organization.IsArchived = true;
        organization.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        organization.LastUpdatedBy = null; // TODO: add user when JWTs are implemented
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync();

        // Log organization archive event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = organization.Id,
            Operation = "archive",
            EntityType = "organization",
            EntityId = organization.Id,
            EntityName = organization.Name,
            Properties = JsonSerializer.Serialize(new { organization.Name }),
        });

        return true;
    }

    /// <summary>
    /// Unarchive a specific organization by ID
    /// </summary>
    /// <param name="organizationId">The ID of the organization to unarchive</param>
    /// <returns>Boolean true on successful unarchive</returns>
    /// <exception cref="KeyNotFoundException">Returned if organization not found</exception>
    public async Task<bool> UnarchiveOrganization(long organizationId)
    {
        var organization = await _context.Organizations.FindAsync(organizationId);

        if (organization == null || !organization.IsArchived)
            throw new KeyNotFoundException($"Organization with id {organizationId} not found");

        // TODO: determine if this needs to be a cascade unarchive instead
        organization.IsArchived = false;
        organization.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync();

        // Log organization archive event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            OrganizationId = organization.Id,
            Operation = "unarchive",
            EntityType = "organization",
            EntityId = organization.Id,
            EntityName = organization.Name,
            Properties = JsonSerializer.Serialize(new { organization.Name }),
        });

        return true;
    }

    /// <summary>
    /// Delete a specific organization by ID
    /// </summary>
    /// <param name="organizationId">The ID of the organization to delete</param>
    /// <returns>Boolean true on successful deletion</returns>
    /// <exception cref="KeyNotFoundException">Returned if organization not found</exception>
    public async Task<bool> DeleteOrganization(long organizationId)
    {
        var organization = await _context.Organizations.FindAsync(organizationId);

        if (organization == null || organization.IsArchived)
            throw new KeyNotFoundException($"Organization with id {organizationId} not found");

        _context.Organizations.Remove(organization);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Add a user to an Organization
    /// </summary>
    /// <param name="organizationId">The ID of the org to add the user to</param>
    /// <param name="userId">The ID of the user to add</param>
    /// <param name="isAdmin">Whether user should be org admin or not</param>
    /// <returns>False if user is already in org, True upon successfully adding user</returns>
    /// <exception cref="KeyNotFoundException">Returned if user or org does not exist</exception>
    public async Task<bool> AddUserToOrganization(long organizationId, long userId, bool isAdmin = false)
    {
        // check if the user is already in the organization
        var existingOrgUser = await _context.OrganizationUsers
            .FirstOrDefaultAsync(ou => ou.OrganizationId == organizationId && ou.UserId == userId);
        if (existingOrgUser != null)
            return false; // org user already exists
        
        // TODO: determine if user account discovery/creation is required
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || user.IsArchived)
            throw new KeyNotFoundException($"User with id {userId} not found");
        
        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId);
        if (organization == null || organization.IsArchived)
            throw new KeyNotFoundException($"Organization with id {organizationId} not found");
        
        // add user to org and assign admin privileges
        var orgUser = new OrganizationUser
        {
            OrganizationId = organizationId,
            UserId = userId,
            IsOrgAdmin = isAdmin,
        };

        _context.OrganizationUsers.Add(orgUser);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Update a user's permissions within an Organization
    /// </summary>
    /// <param name="organizationId">ID of org in which to adjust user perms</param>
    /// <param name="userId">ID of user to adjust</param>
    /// <param name="isAdmin">Admin status to set user to within the org</param>
    /// <returns>True if permissions were updated successfully</returns>
    /// <exception cref="KeyNotFoundException">Returned if user doesn't already exist in org</exception>
    public async Task<bool> SetOrganizationAdminStatus(long organizationId, long userId, bool isAdmin = false)
    {
        // check if the user exists in the organization
        var existingOrgUser = await _context.OrganizationUsers
            .FirstOrDefaultAsync(ou => ou.OrganizationId == organizationId && ou.UserId == userId);

        if (existingOrgUser == null)
            throw new KeyNotFoundException($"User with id {userId} not found in Org with id {organizationId}");

        // set is admin and save to DB
        existingOrgUser.IsOrgAdmin = isAdmin;
        _context.OrganizationUsers.Update(existingOrgUser);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Remove a user from an organization
    /// </summary>
    /// <param name="organizationId">ID of organization</param>
    /// <param name="userId">ID of user</param>
    /// <returns>True if user successfully removed</returns>
    /// <exception cref="KeyNotFoundException">Returned if user doesn't exist in organization</exception>
    public async Task<bool> RemoveUserFromOrganization(long organizationId, long userId)
    {
        // check if the user exists in the organization
        var existingOrgUser = await _context.OrganizationUsers
            .FirstOrDefaultAsync(ou => ou.OrganizationId == organizationId && ou.UserId == userId);

        if (existingOrgUser == null)
            throw new KeyNotFoundException($"User with id {userId} not found in Org with id {organizationId}");

        _context.OrganizationUsers.Remove(existingOrgUser);
        await _context.SaveChangesAsync();

        return true;
    }
}