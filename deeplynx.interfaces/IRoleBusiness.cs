using deeplynx.models;
using deeplynx.datalayer.Models;

namespace deeplynx.interfaces
{
    public interface IRoleBusiness
    {
        IEnumerable<Role> GetAllRoles(long projectId);
        Role GetRole(long projectId, long roleId);
        Role CreateNewRole(Role role);
        Role UpdateRole(long projectId, long roleId, Role role);
        bool DeleteRole(long projectId, long roleId);
    }
}