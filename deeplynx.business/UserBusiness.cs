using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using Microsoft.AspNetCore.SignalR;
using deeplynx.helpers;

namespace deeplynx.business;

public class UserBusiness : IUserBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ICacheBusiness _cacheBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the user operations.</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    public UserBusiness(DeeplynxContext context, ICacheBusiness cacheBusiness)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
    }

    /// <summary>
    /// Retrieves all users
    /// </summary>
    /// <param name="projectId">Optional ID for project</param>
    /// <param name="organizationId">Optional ID for organization</param>
    /// <returns>A list of users, optionally filtered by project or organization</returns>
    public async Task<IEnumerable<UserResponseDto>> GetAllUsers(long? projectId, long? organizationId)
    {
        var users = _context.Users.Where(p => !p.IsArchived);

        if (projectId != null)
        {
            users = users.Where(u =>
                u.ProjectMembers.Any(p => p.ProjectId == projectId && p.UserId == u.Id) ||
                u.Groups.Any(g => g.ProjectMembers.Any(pm => pm.ProjectId == projectId && pm.GroupId == g.Id))
            );
        }

        if (organizationId != null)
        {
            users = users.Where(u => 
                u.OrganizationUsers.Any(ou => ou.OrganizationId == organizationId && ou.UserId == u.Id) ||
                u.Groups.Any(g => g.OrganizationId == organizationId)
            );
        }

        return users.Select(p => new UserResponseDto()
        {
            Id = p.Id,
            SsoId = p.SsoId,
            Name = p.Name,
            Username = p.Username,
            Email = p.Email,
            IsSysAdmin = p.IsSysAdmin,
            IsArchived = p.IsArchived,
            IsActive = p.IsActive,
        });
    }

    /// <summary>
    /// Retrieves a specific user by ID
    /// </summary>
    /// <param name="userId">The ID by which to retrieve the user</param>
    /// <returns>The given user to return</returns>
    /// <exception cref="KeyNotFoundException">Returned if user not found</exception>
    public async Task<UserResponseDto> GetUser(long userId)
    {
        var user = await _context.Users
            .Where(p => p.Id == userId && !p.IsArchived)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new KeyNotFoundException($"User with id {userId} not found");
        }

        return new UserResponseDto()
        {
            Id = user.Id,
            SsoId = user.SsoId,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            IsSysAdmin = user.IsSysAdmin,
            IsArchived = user.IsArchived,
            IsActive = user.IsActive,
        };
    }

    /// <summary>
    /// Creates a new user based on the data transfer object supplied.
    /// </summary>
    /// <param name="dto">A data transfer object with details on the new user to be created.</param>
    /// <returns>The new user which was just created.</returns>
    public async Task<UserResponseDto> CreateUser(CreateUserRequestDto dto)
    {
        // TODO: adjusting is_sys_admin is currently disabled. Enable once route permission protections are in place
        var otherUserHasEmail = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (otherUserHasEmail)
        {
            throw new ArgumentException("User with email already exists");
        }
        
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Username = dto.Username,
            SsoId = dto.SsoId,
            IsActive = dto.IsActive ?? false,
            IsArchived = dto.IsArchived ?? false,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserResponseDto()
        {
            Id = user.Id,
            SsoId = user.SsoId,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            IsSysAdmin = user.IsSysAdmin,
            IsArchived = user.IsArchived,
            IsActive = user.IsActive,
        };
    }

    /// <summary>
    /// Insert user if email not exists, else update user
    /// </summary>
    /// <param name="dto">The user information supplied</param>
    /// <returns>The user which was just updated</returns>
    public async Task<UserResponseDto> RefreshUser(CreateUserRequestDto dto)
    {
        var existingUser = await _context.Users
            .Where(u => u.Email == dto.Email)
            .FirstOrDefaultAsync();
    
        if (existingUser != null)
        {
            // If user exists, update using UpdateUser logic
            var updateDto = new UpdateUserRequestDto
            {
                Name = dto.Name,
                SsoId = dto.SsoId,
                Username = dto.Username,
                IsArchived = dto.IsArchived,
                IsActive = dto.IsActive
            };
        
            return await UpdateUser(existingUser.Id, updateDto);
        }
        else
        {
            // If user does not exist, create
            return await CreateUser(dto);
        }
    }

    /// <summary>
    /// Updates an existing user by ID
    /// </summary>
    /// <param name="userId">The ID of the user to update</param>
    /// <param name="dto">A data transfer object with details on the user to be updated.</param>
    /// <returns>The user which was just updated.</returns>
    /// <exception cref="KeyNotFoundException">Returned if the user was not found.</exception>
    public async Task<UserResponseDto> UpdateUser(long userId, UpdateUserRequestDto dto)
    {
        var user = await _context.Users
            .Where(p => p.Id == userId && !p.IsArchived)
            .FirstOrDefaultAsync();

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.Name = dto.Name ?? user.Name;
        user.SsoId = dto.SsoId ?? user.SsoId;
        user.Username = dto.Username ?? user.Username;
        user.IsArchived = dto.IsArchived ?? user.IsArchived;
        user.IsActive = dto.IsActive ?? user.IsActive;
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return new UserResponseDto()
        {
            Id = user.Id,
            SsoId = user.SsoId,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            IsSysAdmin = user.IsSysAdmin,
            IsArchived = user.IsArchived,
            IsActive = user.IsActive,
        };
    }

    /// <summary>
    /// Delete a user by id.
    /// </summary>
    /// <param name="userId">ID of the user to delete.</param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if user is not found.</exception>
    public async Task<bool> DeleteUser(long userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            throw new KeyNotFoundException($"User with id {userId} not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Archive a user by id.
    /// </summary>
    /// <param name="userId">ID of the user to archive.</param>
    /// <returns>Boolean true on successful archival.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if user is not found.</exception>
    public async Task<bool> ArchiveUser(long userId)
    {
        var user = await _context.Users
            .Where(p => p.Id == userId && !p.IsArchived)
            .FirstOrDefaultAsync();

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.IsArchived = true;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Unarchive a user by id.
    /// </summary>
    /// <param name="userId">ID of the user to unarchive.</param>
    /// <returns>Boolean true when successfully unarchived.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if user is not found.</exception>
    public async Task<bool> UnarchiveUser(long userId)
    {
        var user = await _context.Users
            .Where(p => p.Id == userId && p.IsArchived)
            .FirstOrDefaultAsync();

        if (user == null)
            throw new KeyNotFoundException("Archived user not found.");

        user.IsArchived = false;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }
    
    // TODO: set IsSysAdmin with a business function. Should only be executable by SysAdmins

    /// <summary>
    /// Retrieves data overview counts for a user
    /// </summary>
    /// /// <param name="userId">user id</param>
    /// <returns>Data overview object</returns>
    public async Task<DataOverviewDto> GetUserOverview(long userId)
    {
        // Filtering projects by a user
        var projectsTotal = _context.ProjectMembers
            .Count(p => p.UserId == userId);

        var datasources = _context.DataSources
            .Where(d => !d.IsArchived)
            .Count(d => d.Project.ProjectMembers.Any(u => u.UserId == userId));

        var records = _context.Records
            .Where(d => !d.IsArchived)
            .Count(d => d.Project.ProjectMembers.Any(u => u.UserId == userId));

        var tags = _context.Tags
            .Where(d => !d.IsArchived)
            .Count(d => d.Project.ProjectMembers.Any(u => u.UserId == userId));

        return new DataOverviewDto
        {
            Projects = projectsTotal,
            Connections = datasources,
            Records = records,
            Tags = tags
        };
    }

    /// <summary>
    /// Retrieves current records for projects, ordered by last_updated_at first 
    /// </summary>
    /// <param name="projectId">An array of project ids</param>
    /// <returns>An array of records</returns>
    public async Task<IEnumerable<HistoricalRecordResponseDto>> GetRecentlyAddedRecords(long[] projectIds)
    {
        var records = _context.HistoricalRecords
            .Where(p => projectIds.Contains(p.ProjectId))
            .Where(r => !r.IsArchived)
            .GroupBy(r => r.RecordId)
            .Select(g => g.OrderByDescending(r => r.LastUpdatedAt).First())
            .ToList();
        
        return records
            .Select(r => new HistoricalRecordResponseDto()
            {
                Id = r.RecordId,
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                ClassId = r.ClassId,
                ClassName = r.ClassName,
                DataSourceId = r.DataSourceId,
                DataSourceName = r.DataSourceName,
                ProjectId = r.ProjectId,
                ProjectName = r.ProjectName,
                Tags = r.Tags,
                Description = r.Description,
                LastUpdatedBy = r.LastUpdatedBy,
                IsArchived = r.IsArchived,
                LastUpdatedAt = r.LastUpdatedAt
            });
    }

}