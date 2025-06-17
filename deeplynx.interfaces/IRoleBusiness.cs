using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface IRoleBusiness
    {
        Task<IEnumerable<RoleResponseDto>> GetAllRoles(long projectId);
        Task<RoleResponseDto> GetRole(long projectId, long roleId);
        Task<RoleResponseDto> CreateRole(long projectId, RoleRequestDto role);
        Task<RoleResponseDto> UpdateRole(long projectId, long roleId, RoleRequestDto role);
        Task<bool> DeleteRole(long projectId, long roleId, bool force);
        Task<bool> BulkSoftDeleteRoles(Expression<Func<Role, bool>> predicate);
    }
}