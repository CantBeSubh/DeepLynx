using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IPermissionBusiness
{
    Task<IEnumerable<PermissionResponseDto>> GetAllPermissions(
        long? labelId, long? projectId, long? organizationId, bool hideArchived = true);
    Task<PermissionResponseDto> GetPermission(long? organizationId, long? projectI, long permissionId, bool hideArchived = true);
    Task<PermissionResponseDto> CreatePermission(
        long currentUserId, CreatePermissionRequestDto role, long? projectId, long organizationId);
    Task<PermissionResponseDto> UpdatePermission(long organizationId, long? projectId, long currentUserId, long permissionId, UpdatePermissionRequestDto role);
    Task<bool> ArchivePermission(long organizationId, long? projectId, long currentUserId, long permissionId);
    Task<bool> UnarchivePermission(long organizationId, long? projectId, long currentUserId, long permissionId);
    Task<bool> DeletePermission(long organizationId, long? projectId, long currentUserId, long permissionId);
}