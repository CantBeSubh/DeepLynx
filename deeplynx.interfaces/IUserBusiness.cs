using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface IUserBusiness
{
    Task<IEnumerable<UserResponseDto>> GetAllUsers(long? projectId);
    Task<UserResponseDto> GetUser(long userId);
    Task<UserResponseDto> CreateUser(CreateUserRequestDto dto);
    Task<UserResponseDto> UpdateUser(long userId, UpdateUserRequestDto dto);
    Task<bool> DeleteUser(long userId);
    Task<bool> ArchiveUser(long userId);
    
    Task<bool>  AddUserToProject(long userId, long projectId);
    Task<bool>  RemoveUserFromProject(long userId, long projectId);
    
    Task<DataOverviewDto> GetUserOverview(long userId);
    Task<bool> UnarchiveUser(long userId);
    
    Task<IEnumerable<HistoricalRecordResponseDto>> GetRecentlyAddedRecords(
        long[] projectId);
}