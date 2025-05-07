using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business
{
    public class RoleBusiness : IRoleBusiness
    {
        private readonly DeeplynxContext _context;

        public RoleBusiness(DeeplynxContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllRoles(long projectId)
        {
            return await _context.Roles
                .Where(r => r.ProjectId == projectId && r.DeletedAt == null)
                .ToListAsync();
        }

        public async Task<Role> GetRole(long projectId, long roleId)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && r.ProjectId == projectId && r.DeletedAt == null) ??
                throw new KeyNotFoundException($"Role with id {roleId} not found");
        }

        public async Task<Role> CreateRole(long projectId, RoleRequestDto dto)
        {
            var role = new Role
            {
                Name = dto.Name,
                ProjectId = projectId
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return role;
        }

        public async Task<Role> UpdateRole(long projectId, long roleId, RoleRequestDto dto)
        {
            var role = await GetRole(projectId, roleId);

            role.Name = dto.Name;

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            return role;
        }

        public async Task<bool> DeleteRole(long projectId, long roleId)
        {
            var role = await GetRole(projectId, roleId);

            //soft delete
            role.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified); 
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}