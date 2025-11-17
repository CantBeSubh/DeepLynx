using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IPermissionBusiness
{
    Task<IEnumerable<PermissionResponseDto>> GetAllPermissions(
        long? labelId, long? projectId, long? organizationId, bool hideArchived = true);
    Task<PermissionResponseDto> GetPermission(long permissionId, bool hideArchived = true);
    Task<PermissionResponseDto> CreatePermission(
        long currentUserId, CreatePermissionRequestDto role, long? projectId, long? organizationId);
    Task<PermissionResponseDto> UpdatePermission(long currentUserId, long permissionId, UpdatePermissionRequestDto role);
    Task<bool> ArchivePermission(long currentUserId, long permissionId);
    Task<bool> UnarchivePermission(long currentUserId, long permissionId);
    Task<bool> DeletePermission(long permissionId);
}