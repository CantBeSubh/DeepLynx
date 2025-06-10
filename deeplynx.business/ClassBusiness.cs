using System.Transactions;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
            .Where(c => c.ProjectId == projectId && c.DeletedAt == null)
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
            .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Id == classId && c.DeletedAt == null);
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
        var project=await _context.Projects.FirstOrDefaultAsync(p=>p.Id == projectId && p.DeletedAt == null);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }
        
        var newClass = new Class
        {
            ProjectId = projectId,
            Name = dto.Name,
            Description = dto.Description,
            Uuid = dto.Uuid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null , // TODO: Implement user ID here when JWT tokens are ready
            ModifiedBy = null  // TODO: Implement user ID here when JWT tokens are ready
           
        };

        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();

        return  new ClassResponseDto
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
        if (updatedClass == null || updatedClass.ProjectId != projectId || updatedClass.DeletedAt is not null)
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

    public async Task<bool> DeleteClass(long projectId, long classId, bool force=false)
    {
        var dbClass = await _context.Classes.FindAsync(classId);
        
        if (dbClass == null || dbClass.ProjectId != projectId || dbClass.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Class with id {classId} not found");
        }

        if (force)
        {
            _context.Classes.Remove(dbClass);
        }
        else
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            var softDeleteTasks = new List<Func<Task<bool>>>
            {
                () => _edgeMappingBusiness.BulkSoftDeleteEdgeMappings("class", [classId]), 
                () => _recordBusiness.BulkSoftDeleteRecords("class", [classId], transaction), 
                () => _relationshipBusiness.BulkSoftDeleteRelationships("class", [classId]),
                () => _recordMappingBusiness.BulkSoftDeleteRecordMappings("class", [classId])
            };

            foreach (var task in softDeleteTasks)
            {
                bool result = await task();
                if (!result)
                {
                    await transaction.RollbackAsync();
                    throw new ProjectDependencyDeletionException($"error while deleting downstream dependants for class {classId}");
                }
            }

            dbClass.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.Classes.Update(dbClass);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
        }
        return true;
    }

    /// <summary>
    /// Bulk Soft Delete classes by a specific upstream domain. Used to avoid repeating functions.
    /// </summary>
    /// <param name="domainType">The type of domain which is calling this function</param>
    /// <param name="domainId">The ID of the upstream domain calling this function</param>
    /// <param name="transaction">(Optional) a transaction passed in from the parent to ensure ACID compliance</param>
    /// <returns>Boolean true on successful deletion</returns>
    public async Task<bool> BulkSoftDeleteClasses(string domainType, long domainId, IDbContextTransaction? transaction)
    {
        try
        {
            var classQuery = _context.Classes.Where(c => c.DeletedAt == null);

            if (domainType == "project")
            {
                classQuery = classQuery.Where(c => c.ProjectId == domainId);
            }
                    
            var classes = await classQuery.ToListAsync();
            
            // start a database transaction to ensure deletion changes are rolled back if errors occur
            var commit = false; // variable to indicate whether we can commit or if parent should commit transaction
            if (transaction == null)
            {
                commit = true;
                transaction = await _context.Database.BeginTransactionAsync();
            }
            
            var softDeleteTasks = new List<Func<Task<bool>>>
            {
                () => _edgeMappingBusiness.BulkSoftDeleteEdgeMappings("class", classes.Select(c => c.Id)), 
                () => _recordBusiness.BulkSoftDeleteRecords("class", classes.Select(c => c.Id), transaction), 
                () => _relationshipBusiness.BulkSoftDeleteRelationships("class", classes.Select(c => c.Id)),
                () => _recordMappingBusiness.BulkSoftDeleteRecordMappings("class", classes.Select(c => c.Id))
            };

            foreach (var task in softDeleteTasks)
            {
                bool result = await task();
                if (!result)
                {
                    await transaction.RollbackAsync();
                    throw new ProjectDependencyDeletionException($"error while deleting downstream class dependants");
                }
            }
                
            foreach (var c in classes)
            {
                c.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }
                
            await _context.SaveChangesAsync();
            return true;
                
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting classes for domain {domainType} with id {domainId}: {exc}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
}