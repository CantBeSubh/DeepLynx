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
    /// /// <param name="projectId">Optional ID for project</param>
    /// <returns>A list of users, or a list of users for a project</returns>
    public async Task<IEnumerable<UserResponseDto>> GetAllUsers(long? projectId)
    {
        List<User> users;

        if (projectId == null)
        {
            users = await _context.Users
                .Where(p => !p.IsArchived)
                .ToListAsync();
        }
        else
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);

            users = await _context.Users
                .Where(u => u.Projects.Any(p => p.Id == projectId))
                .ToListAsync();

        }

        return users.Select(p => new UserResponseDto()
        {
            Id = p.Id,
            Name = p.Name,
            Email = p.Email,
            IsSysAdmin = p.IsSysAdmin
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
            Name = user.Name,
            Email = user.Email,
            IsSysAdmin = user.IsSysAdmin
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
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        if (dto.ProjectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, dto.ProjectId.Value, _cacheBusiness);

            var project = _context.Projects.FirstOrDefault(p => p.Id == dto.ProjectId);

            if (project != null)
            {
                project.Users.Add(user);
                _context.SaveChanges();
            }
        }
     

        return new UserResponseDto()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            IsSysAdmin = user.IsSysAdmin
        };
    }

    /// <summary>
    /// Updates an existing user by ID
    /// </summary>
    /// <param name="userId">The ID of the user to update</param>
    /// <param name="dto">A data transfer object with details on the user to be updated.</param>
    /// <returns>The user which was just updated.</returns>
    /// <exception cref="KeyNotFoundException">Returned if the user was not found.</exception>
    /// TODO: Decide if we want to update to null if null value is given in the DTO
    /// TODO: Decide if we want to allow add/remove to a project in this update method 
    public async Task<UserResponseDto> UpdateUser(long userId, UpdateUserRequestDto dto)
    {
        var user = await _context.Users
            .Where(p => p.Id == userId && !p.IsArchived)
            .FirstOrDefaultAsync();

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.Name = dto.Name ?? user.Name;
        if (dto.Email != null)
        {
            var otherUserHasEmail = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (otherUserHasEmail)
            {
                throw new ArgumentException("User with email already exists");
            }
            user.Email = dto.Email;
        }
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return new UserResponseDto()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            IsSysAdmin = user.IsSysAdmin,
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

    /// <summary>
    /// Retrieves data overview counts for a user
    /// </summary>
    /// /// <param name="userId">user id</param>
    /// <returns>Data overview object</returns>
    public async Task<DataOverviewDto> GetUserOverview(long userId)
    {

        // Filtering projects by a user
        var projectsTotal = _context.Projects
            .Include(p => p.Users)
            .Count(p => p.Users.Any(u => u.Id == userId));

        var datasources = _context.DataSources
            .Count(d => d.Project.Users.Any(u => u.Id == userId));

        var records = _context.Records
            .Count(d => d.Project.Users.Any(u => u.Id == userId));

        var tags = _context.Tags
            .Count(d => d.Project.Users.Any(u => u.Id == userId));

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