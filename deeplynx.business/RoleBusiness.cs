using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
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
            var project= await _context.Projects.FirstOrDefaultAsync(p=>p.Id == projectId && p.ArchivedAt == null);
            if (project == null)
            {
                throw new KeyNotFoundException($"Project with id {projectId} not found");
            }
            
            return await _context.Roles
                .Where(r => r.ProjectId == projectId && r.ArchivedAt == null)
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
                .Where(r => r.Id == roleId && r.ProjectId == projectId && r.ArchivedAt == null)
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
            if (role == null || role.ProjectId != projectId || role.ArchivedAt is not null)
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
        /// <param name="force">Indicates whether to force delete the role if true.</param>
        public async Task<bool> DeleteRole(long projectId, long roleId, bool force)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null || role.ProjectId != projectId || role.ArchivedAt is not null)
            {
                throw new KeyNotFoundException($"Role with id {roleId} not found");
            }

            if (force)
            {
                _context.Roles.Remove(role);
            }
            else
            {
                //soft delete
                role.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified); 
                _context.Roles.Update(role);
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Bulk Soft Delete roles by a specific upstream domain. Used to avoid repeating functions.
        /// </summary>
        /// <param name="predicate">an anonymous function that allows the context to be filtered appropriately</param>
        /// <returns>Boolean true on successful deletion</returns>
        public async Task<bool> BulkSoftDeleteRoles(Expression<Func<Role, bool>> predicate)
        {
            try
            {
                // search for roles matching the passed-in predicate (filter) to be updated
                var rContext = _context.Roles
                    .Where(d => d.ArchivedAt == null)
                    .Where(predicate);

                var roles = await rContext.ToListAsync();
            
                if (roles.Count == 0)
                {
                    // return early if no roles are to be deleted
                    return true;
                }

                // bulk update the results of the query to set the archived_at date
                var updated = await rContext.ExecuteUpdateAsync(setters => setters
                    .SetProperty(ds => ds.ArchivedAt, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)));

                // if we found roles to update, but weren't successful in updating, throw an error
                if (updated == 0)
                {
                    throw new DependencyDeletionException("Roles found but were not deleted");
                }

                // save changes and commit transaction to close it
                await _context.SaveChangesAsync();
                return true;
                
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting roles: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return false;
            }
        }
    }
}