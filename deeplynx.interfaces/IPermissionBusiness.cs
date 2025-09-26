using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IPermissionBusiness
{
    Task<IEnumerable<PermissionResponseDto>> GetAllPermissions(
        long? labelId, long? projectId, long? organizationId, bool hideArchived = true);
    Task<PermissionResponseDto> GetPermission(long permissionId, bool hideArchived = true);
    Task<PermissionResponseDto> CreatePermission(
        CreatePermissionRequestDto role, long? projectId, long? organizationId);
    Task<PermissionResponseDto> UpdatePermission(long permissionId, UpdatePermissionRequestDto role);
    Task<bool> ArchivePermission(long permissionId);
    Task<bool> UnarchivePermission(long permissionId);
    Task<bool> DeletePermission(long permissionId);
}