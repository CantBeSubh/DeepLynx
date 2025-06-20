using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using deeplynx.helpers;

namespace deeplynx.business;

public class ClassBusiness : IClassBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IEdgeMappingBusiness _edgeMappingBusiness;
    private readonly IRecordBusiness _recordBusiness;
    private readonly IRecordMappingBusiness _recordMappingBusiness;
    private readonly IRelationshipBusiness _relationshipBusiness;

    public ClassBusiness(
        DeeplynxContext context,
        IEdgeMappingBusiness edgeMappingBusiness,
        IRecordBusiness recordBusiness,
        IRecordMappingBusiness recordMappingBusiness,
        IRelationshipBusiness relationshipBusiness
        )
    {
        _context = context;
        _edgeMappingBusiness = edgeMappingBusiness;
        _recordBusiness = recordBusiness;
        _recordMappingBusiness = recordMappingBusiness;
        _relationshipBusiness = relationshipBusiness;
    }

    public async Task<IEnumerable<ClassResponseDto>> GetAllClasses(long projectId)
    {
        return await _context.Classes
            .Where(c => c.ProjectId == projectId && c.ArchivedAt == null)
            .Select(c => new ClassResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Uuid = c.Uuid,
                ProjectId = c.ProjectId,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                ModifiedBy = c.ModifiedBy,
                ModifiedAt = c.ModifiedAt
            })
            .ToListAsync();
    }

    public async Task<ClassResponseDto> GetClass(long projectId, long classId)
    {
        var newClass = await _context.Classes
            .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Id == classId && c.ArchivedAt == null);
        if (newClass == null)
        {
            throw new KeyNotFoundException($"Class with id {classId} not found");
        }

        return new ClassResponseDto
        {
            Id = newClass.Id,
            Name = newClass.Name,
            Description = newClass.Description,
            Uuid = newClass.Uuid,
            ProjectId = newClass.ProjectId,
            CreatedBy = newClass.CreatedBy,
            CreatedAt = newClass.CreatedAt,
            ModifiedBy = newClass.ModifiedBy,
            ModifiedAt = newClass.ModifiedAt
        };
    }

    public async Task<ClassResponseDto> CreateClass(long projectId, ClassRequestDto dto)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.ArchivedAt == null);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }

        ValidationHelper.ValidateModel(dto);

        var existingClass = await _context.Classes.FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Name == dto.Name);
        if (existingClass != null)
        {
            throw new Exception($"Class for project {projectId} with name {dto.Name} already exists");
        }

        var newClass = new Class
        {
            ProjectId = projectId,
            Name = dto.Name,
            Description = dto.Description,
            Uuid = dto.Uuid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null, // TODO: Implement user ID here when JWT tokens are ready
            ModifiedBy = null  // TODO: Implement user ID here when JWT tokens are ready

        };

        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();

        return new ClassResponseDto
        {
            Id = newClass.Id,
            Name = newClass.Name,
            Description = newClass.Description,
            Uuid = newClass.Uuid,
            ProjectId = newClass.ProjectId,
            CreatedBy = newClass.CreatedBy,
            CreatedAt = newClass.CreatedAt,
            ModifiedBy = newClass.ModifiedBy,
            ModifiedAt = newClass.ModifiedAt
        };
    }

    public async Task<ClassResponseDto> UpdateClass(long projectId, long classId, ClassRequestDto dto)
    {
        var updatedClass = await _context.Classes.FindAsync(classId);
        if (updatedClass == null || updatedClass.ProjectId != projectId || updatedClass.ArchivedAt is not null)
        {
            throw new KeyNotFoundException($"Class with id {classId} not found");
        }

        updatedClass.ProjectId = projectId;
        updatedClass.Name = dto.Name;
        updatedClass.Description = dto.Description;
        updatedClass.Uuid = dto.Uuid;
        updatedClass.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        updatedClass.ModifiedBy = null;  // TODO: Implement user ID here when JWT tokens are ready
        updatedClass.CreatedBy = null; // TODO: Implement user ID here when JWT tokens are ready
        updatedClass.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        _context.Classes.Update(updatedClass);
        await _context.SaveChangesAsync();

        return new ClassResponseDto
        {
            Id = updatedClass.Id,
            Name = updatedClass.Name,
            Description = updatedClass.Description,
            Uuid = updatedClass.Uuid,
            ProjectId = updatedClass.ProjectId,
            CreatedBy = updatedClass.CreatedBy,
            CreatedAt = updatedClass.CreatedAt,
            ModifiedBy = updatedClass.ModifiedBy,
            ModifiedAt = updatedClass.ModifiedAt,

        };
    }

    /// <summary>
    /// Delete a class by id.
    /// </summary>
    /// <param name="projectId">ID of the project to which the class belongs.</param>
    /// <param name="classId">ID of the class to delete.</param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if class is not found.</exception>
    public async Task<bool> DeleteClass(long projectId, long classId)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null || project.ArchivedAt is not null)
            throw new KeyNotFoundException($"Project with id {projectId} not found.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Archive (soft delete) a class by id. This also archives downstream dependents.
    /// </summary>
    /// <param name="classId">ID of the class to archive.</param>
    /// <returns>Boolean true on successful archival.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if class is not found.</exception>
    /// <exception cref="DependencyDeletionException">Thrown if archival fails.</exception>
    public async Task<bool> ArchiveClass(long projectId, long classId)
    {
        // using dbClass since "class" is a reserved word
        var dbClass = await _context.Classes.FindAsync(classId);

        if (dbClass == null || dbClass.ProjectId != projectId || dbClass.ArchivedAt is not null)
            throw new KeyNotFoundException("Class not found.");

        // set archivedAt timestamp
        var archivedAt = DateTime.UtcNow;

        // run archive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the archive class procedure, which archives this class
                // and all child objects with class_id as a foreign key
                var archived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.archive_class({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)", classId, archivedAt);

                if (archived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException($"unable to archive class {classId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException($"unable to archive class {classId} or its downstream dependents: {exc}");
            }
        }
    }
}