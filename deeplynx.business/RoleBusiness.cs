using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.business
{
    public class RoleBusiness : IRoleBusiness
    {
        private readonly DeepLynxContext _context;

        public RoleBusiness(DeepLynxContext context)
        {
            _context = context;
        }

        public IEnumerable<Role> GetAllRoles(long projectId)
        {
            return _context.Roles.Where(r => r.ProjectId == projectId && r.DeletedAt == null).Select(r => new Role
            {
                Id = r.Id,
                Name = r.Name,
                ProjectId = r.ProjectId
            }).ToList();
        }

        public Role GetRole(long projectId, long roleId)
        {
            // First or Default should only return a single record I believe, so should be good on any multiple returns issue
            var role = _context.Roles.FirstOrDefault(r => r.Id == roleId && r.ProjectId == projectId && r.DeletedAt == null);

            if (role == null)
                throw new Exception("Role not found");

            return new Role
            {
                Id = role.Id,
                Name = role.Name,
                ProjectId = role.ProjectId
            };
        }

        public Role CreateNewRole(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var newRole = new datalayer.Models.Role
            {
                Name = role.Name,
                ProjectId = role.ProjectId
            };

            _context.Roles.Add(newRole);
            _context.SaveChanges();

            role.Id = newRole.Id;
            return role;
        }

        public Role UpdateRole(long projectId, long roleId, Role role)
        {
            var existing = _context.Roles.FirstOrDefault(r => r.Id == roleId && r.ProjectId == projectId && r.DeletedAt == null);

            if (existing == null)
                throw new Exception("Role not found");

            existing.Name = role.Name;

            _context.SaveChanges();

            return role;
        }

        public bool DeleteRole(long projectId, long roleId)
        {
            var existing = _context.Roles.FirstOrDefault(r => r.Id == roleId && r.ProjectId == projectId && r.DeletedAt == null);
            if (existing == null)
                return false;

            //soft delete
            existing.DeletedAt = DateTime.UtcNow; 
            _context.SaveChanges();

            return true;
        }
    }
}