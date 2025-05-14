using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business
{
    public class RoleBusiness : IRoleBusiness
    {
        private readonly DeeplynxContext _context;

        /// <summary>
        ///  Initializes a new instance of the <see cref="RoleBusiness"/> class.
        /// </summary>
        /// <param name="context">The database context used for role operations.</param>
        public RoleBusiness(DeeplynxContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all roles for a specific project
        /// </summary>
        /// <param name="projectId">The ID of the project whose roles are to be retrieved</param>
        /// <returns>A list of roles belonging to the project.</returns>
        public async Task<IEnumerable<RoleResponseDto>> GetAllRoles(long projectId)
        {
            return await _context.Roles
                .Where(r => r.ProjectId == projectId && r.DeletedAt == null)
                .Select(r => new RoleResponseDto()
                {
                    Id = r.Id,
                    Name = r.Name,
                    ProjectId = r.ProjectId,
                    CreatedBy = r.CreatedBy,
                    CreatedAt = r.CreatedAt,
                    ModifiedBy = r.ModifiedBy,
                    ModifiedAt = r.ModifiedAt
                })
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific role by its ID.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the role belongs.</param>
        /// <param name="roleId">The ID of the role to retrieve.</param>
        /// <returns>The role with its details.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the role is not found.</exception>
        public async Task<RoleResponseDto> GetRole(long projectId, long roleId)
        {
            var role = await _context.Roles
                .Where(r => r.Id == roleId && r.ProjectId == projectId && r.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (role == null)
            {
                throw new KeyNotFoundException($"Role with id {roleId} not found");
            }

            return new RoleResponseDto
            {
                Id = role.Id,
                Name = role.Name,
                ProjectId = role.ProjectId,
                CreatedBy = role.CreatedBy,
                CreatedAt = role.CreatedAt,
                ModifiedBy = role.ModifiedBy,
                ModifiedAt = role.ModifiedAt
            };
        }

        /// <summary>
        /// Asynchronously creates a new role for a specified project.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the role belongs.</param>
        /// <param name="dto">The role request data transfer object containing role details</param>
        /// <returns>The created role response DTO with saved details.</returns>
        public async Task<RoleResponseDto> CreateRole(long projectId, RoleRequestDto dto)
        {
            // Validate 'Name' field
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Name is required and cannot be empty.");
            }
            
            var role = new Role
            {
                Name = dto.Name,
                ProjectId = projectId,
                CreatedBy = null, // TODO: handled in future by JWT.
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return new RoleResponseDto
            {
                Id = role.Id,
                Name = role.Name,
                ProjectId = role.ProjectId,
                CreatedBy = role.CreatedBy,
                CreatedAt = role.CreatedAt
            };
        }

        /// <summary>
        /// Updates an existing role for a specified project.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the role belongs.</param>
        /// <param name="roleId">The ID of the role to update.</param>
        /// <param name="dto">The role request data transfer object containing updated role details.</param>
        /// <returns>The updated role response DTO with its details.</returns>
        public async Task<RoleResponseDto> UpdateRole(long projectId, long roleId, RoleRequestDto dto)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null || role.ProjectId != projectId || role.DeletedAt is not null)
            {
                throw new KeyNotFoundException($"Role with id {roleId} not found");
            }
            
            // Validate 'Name' field
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Name is required and cannot be empty.");
            }

            role.Name = dto.Name;
            role.ModifiedBy = null; // TODO: handled in future by JWT.
            role.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            
            return new RoleResponseDto
            {
                Id = role.Id,
                Name = role.Name,
                ProjectId = role.ProjectId,
                CreatedBy = role.CreatedBy,
                CreatedAt = role.CreatedAt,
                ModifiedBy = role.ModifiedBy,
                ModifiedAt = role.ModifiedAt
            };
        }

        /// <summary>
        /// Deletes a specific role by its ID for a specified project.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the role belongs.</param>
        /// <param name="roleId">The ID of the role to delete.</param>
        public async Task<bool> DeleteRole(long projectId, long roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null || role.ProjectId != projectId || role.DeletedAt is not null)
            {
                throw new KeyNotFoundException($"Role with id {roleId} not found");
            }

            //soft delete
            role.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified); 
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}