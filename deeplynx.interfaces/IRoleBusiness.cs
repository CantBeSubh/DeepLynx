using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRoleBusiness
{
    Task<IEnumerable<RoleResponseDto>> GetAllRoles(long? projectId, long? organizationId, bool hideArchived = true);
    Task<RoleResponseDto> GetRole(long roleId, bool hideArchived = true);
    Task<RoleResponseDto> CreateRole(long currentUserId, CreateRoleRequestDto role, long? projectId, long? organizationId);
    Task<List<RoleResponseDto>> BulkCreateRoles(long currentUserId, long projectId, List<CreateRoleRequestDto> dtos);
    Task<RoleResponseDto> UpdateRole(long currentUserId, long roleId, UpdateRoleRequestDto role);
    Task<bool> ArchiveRole(long currentUserId, long roleId);
    Task<bool> UnarchiveRole(long currentUserId, long roleId);
    Task<bool> DeleteRole(long roleId);
    Task<IEnumerable<PermissionResponseDto>> GetPermissionsByRole(long roleId);
    Task<bool> AddPermissionToRole(long roleId, long permissionId);
    Task<bool> RemovePermissionFromRole(long roleId, long permissionId);
    Task<bool> SetPermissionsForRole(long roleId, long[] permissionIds);
    Task<bool> SetPermissionsByPattern(long roleId, Dictionary<string, string[]> permissionPatterns);
}