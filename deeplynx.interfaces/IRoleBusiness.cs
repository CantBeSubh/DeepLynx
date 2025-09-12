using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRoleBusiness
{
    Task<IEnumerable<RoleResponseDto>> GetAllRoles(long? projectId, long? organizationId, bool hideArchived = true);
    Task<RoleResponseDto> GetRole(long roleId, long? projectId, long? organizationId, bool hideArchived = true);
    Task<RoleResponseDto> CreateRole(CreateRoleRequestDto role, long? projectId, long? organizationId);
    Task<RoleResponseDto> UpdateRole(long roleId, UpdateRoleRequestDto role);
    Task<bool> ArchiveRole(long roleId);
    Task<bool> UnarchiveRole(long roleId);
    Task<bool> DeleteRole(long roleId);
}