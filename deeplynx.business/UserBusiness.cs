using Microsoft.EntityFrameworkCore;
using System.Transactions;

using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;

namespace deeplynx.business;

public class UserBusiness : IUserBusiness
{
    private readonly DeeplynxContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UserBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the record mapping operations.</param>
    public UserBusiness(DeeplynxContext context)
    {
        _context = context;
   
    }

    /// <summary>
    /// Retrieves all users
    /// </summary>
    /// <returns>A list of users, or a list of users for a project</returns>
    public async Task<IEnumerable<UserResponseDto>> GetAllUsers(long? projectId)
    {
        List<User> users;
        if (projectId == null)
        {
            users = await _context.Users.Where(p => p.ArchivedAt == null).ToListAsync();
        }
        else
        {
            users = await _context.Users.Where(p => p.ArchivedAt == null && p.projectId == projectId).ToListAsync();
        }
        
        return users
            .Select(p => new UserResponseDto()
            {
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email
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
            .Where(p => p.Id == userId && p.ArchivedAt == null)
            .FirstOrDefaultAsync();
    
        if (user == null)
        {
            throw new KeyNotFoundException($"User with id {userId} not found");
        }
    
        return new UserResponseDto()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ArchivedAt = user.ArchivedAt,
        };
    }
    
    /// <summary>
    /// Creates a new user based on the data transfer object supplied.
    /// </summary>
    /// <param name="dto">A data transfer object with details on the new user to be created.</param>
    /// <returns>The new user which was just created.</returns>
    public async Task<UserResponseDto> CreateUser(UserRequestDto dto)
    {
        var user = new User
        {
          FirstName = dto.FirstName,
          LastName = dto.LastName,
          Email = dto.Email,
        };
    
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return new UserResponseDto()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
        };
    }
    
    /// <summary>
    /// Updates an existing user by ID
    /// </summary>
    /// <param name="userId">The ID of the user to update</param>
    /// <param name="dto">A data transfer object with details on the user to be updated.</param>
    /// <returns>The user which was just updated.</returns>
    /// <exception cref="KeyNotFoundException">Returned if the user was not found.</exception>
    public async Task<UserResponseDto> UpdateUser(long userId, UserRequestDto dto)
    {
        var user = await _context.Users
            .Where(p => p.Id == userId && p.ArchivedAt == null)
            .FirstOrDefaultAsync();
        
        if (user == null)
            throw new KeyNotFoundException("User not found.");
        
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
    
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    
        return new UserResponseDto()
        {
           FirstName = user.FirstName,
           LastName = user.LastName,
           Email = user.Email,
        };
    }
    
    /// <summary>
    /// Delete a user by id.
    /// </summary>
    /// <param name="userId">ID of the user to delete.</param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if user is not found.</exception>
    public async void DeleteUser(long userId)
    {
        var user = await _context.Users
            .Where(p => p.Id == userId && p.ArchivedAt == null)
            .FirstOrDefaultAsync();
        
        if (user == null)
            throw new KeyNotFoundException("User not found.");
    
        _context.Projects.Remove(user);
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Archive a user by id.
    /// </summary>
    /// <param name="userId">ID of the user to archive.</param>
    /// <returns>Boolean true on successful archival.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if user is not found.</exception>
    public async void ArchiveUser(long userId)
    {
        var user = await _context.Users
            .Where(p => p.Id == userId && p.ArchivedAt == null)
            .FirstOrDefaultAsync();
        
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.archivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
