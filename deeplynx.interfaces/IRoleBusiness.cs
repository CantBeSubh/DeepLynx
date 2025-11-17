using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRoleBusiness
{
    Task<IEnumerable<RoleResponseDto>> GetAllRoles(long? organizationId, long? projectId, bool hideArchived = true);
    Task<RoleResponseDto> GetRole(long roleId, bool hideArchived = true);
    Task<RoleResponseDto> CreateRole(CreateRoleRequestDto role, long organizationId, long? projectId);
    Task<List<RoleResponseDto>> BulkCreateRoles(long organizationId, long? projectId, List<CreateRoleRequestDto> dtos);
    Task<RoleResponseDto> UpdateRole(long roleId, UpdateRoleRequestDto role);
    Task<bool> ArchiveRole(long roleId);
    Task<bool> UnarchiveRole(long roleId);
    Task<bool> DeleteRole(long roleId);
    Task<IEnumerable<PermissionResponseDto>> GetPermissionsByRole(long roleId);
    Task<bool> AddPermissionToRole(long roleId, long permissionId);
    Task<bool> RemovePermissionFromRole(long roleId, long permissionId);
    Task<bool> SetPermissionsForRole(long roleId, long[] permissionIds);
    Task<bool> SetPermissionsByPattern(long roleId, Dictionary<string, string[]> permissionPatterns);
}