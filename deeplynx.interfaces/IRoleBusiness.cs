using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface IRoleBusiness
    {
        Task<IEnumerable<Role>> GetAllRoles(long projectId);
        Task<Role> GetRole(long projectId, long roleId);
        Task<Role> CreateRole(long projectId, RoleRequestDto role);
        Task<Role> UpdateRole(long projectId, long roleId, RoleRequestDto role);
        Task<bool> DeleteRole(long projectId, long roleId);
    }
}