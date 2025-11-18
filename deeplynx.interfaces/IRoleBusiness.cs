using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRoleBusiness
{
    Task<IEnumerable<RoleResponseDto>> GetAllRoles(long organizationId, long? projectId, bool hideArchived = true);
    Task<RoleResponseDto> GetRole(long roleId, long organizationId, long? projectId, bool hideArchived = true);
    Task<RoleResponseDto> CreateRole(long currentUserId, CreateRoleRequestDto role, long organizationId, long? projectId);
    Task<List<RoleResponseDto>> BulkCreateRoles(long currentUserId, long organizationId, long? projectId, List<CreateRoleRequestDto> dtos);
    Task<RoleResponseDto> UpdateRole(long currentUserId, long roleId, long organizationId, long? projectId, UpdateRoleRequestDto role);
    Task<bool> ArchiveRole(long currentUserId, long roleId, long organizationId, long? projectId);
    Task<bool> UnarchiveRole(long currentUserId, long roleId, long organizationId, long? projectId);
    Task<bool> DeleteRole(long currentUserId, long roleId, long organizationId, long? projectId);
    Task<IEnumerable<PermissionResponseDto>> GetPermissionsByRole(long roleId, long organizationId, long? projectId);
    Task<bool> AddPermissionToRole(long roleId, long permissionId, long organizationId, long? projectId);
    Task<bool> RemovePermissionFromRole(long roleId, long permissionId, long organizationId, long? projectId);
    Task<bool> SetPermissionsForRole(long roleId, long[] permissionIds, long organizationId, long? projectId);
    Task<bool> SetPermissionsByPattern(long roleId, Dictionary<string, string[]> permissionPatterns, long organizationId, long? projectId);
}