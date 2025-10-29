using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface IUserBusiness
{
    Task<IEnumerable<UserResponseDto>> GetAllUsers(long? projectId, long? organizationId);
    Task<UserResponseDto> GetUser(long userId);
    Task<UserResponseDto> GetLocalDevUser();
    Task<UserResponseDto> CreateUser(CreateUserRequestDto dto);
    Task<UserResponseDto> UpdateUser(long userId, UpdateUserRequestDto dto);
    Task<bool> DeleteUser(long userId);
    Task<bool> ArchiveUser(long userId);
    Task<DataOverviewDto> GetUserOverview(long userId);
    Task<bool> UnarchiveUser(long userId);
    Task<bool> SetSysAdmin(long authorizerId, long candidateId);
    Task<IEnumerable<HistoricalRecordResponseDto>> GetRecentlyAddedRecords(
        long[] projectId);
}