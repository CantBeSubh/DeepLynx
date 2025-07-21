using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface IUserBusiness
{
    Task<IEnumerable<UserResponseDto>> GetAllUsers(long? projectId);
    Task<UserResponseDto> GetUser(long userId);
    Task<UserResponseDto> CreateUser(UserRequestDto dto);
    Task<UserResponseDto> UpdateUser(long userId, UserRequestDto dto);
    Task<bool> DeleteUser(long userId);
    Task<bool> ArchiveUser(long userId);
    
    Task<bool>  AddUserToProject(long userId, long projectId);
    Task<bool>  RemoveUserFromProject(long userId, long projectId);
    
    Task<DataOverviewDto> GetUserOverview(long userId);
    Task<bool> UnarchiveUser(long userId);
}