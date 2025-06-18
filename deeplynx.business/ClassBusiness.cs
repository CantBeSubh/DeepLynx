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

        ValidationHelper.ValidateModel(dto);
        
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

    public async Task<bool> DeleteClass(long projectId, long classId, bool force = false)
    {
        var dbClass = await _context.Classes.FindAsync(classId);
        
        if (dbClass == null || dbClass.ProjectId != projectId || dbClass.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Class with id {classId} not found");
        }

        if (force)
        {
            _context.Classes.Remove(dbClass);
            await _context.SaveChangesAsync();
        }
        else
        {
            try
            {
                var transaction = await _context.Database.BeginTransactionAsync();
                await SoftDeleteClasses(c => c.Id == classId, transaction);
                await transaction.CommitAsync();
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting data source: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Bulk Soft Delete classes by a specific upstream domain. Used to avoid repeating functions.
    /// </summary>
    /// <param name="predicate">an anonymous function that allows the context to be filtered appropriately</param>
    /// <param name="transaction">(Optional) a transaction passed in from the parent to ensure ACID compliance</param>
    /// <returns>Boolean true on successful deletion</returns>
    public async Task<bool> BulkSoftDeleteClasses(
        Expression<Func<Class, bool>> predicate, 
        IDbContextTransaction? transaction)
    {
        try
        {
            await SoftDeleteClasses(predicate, transaction);
            return true;
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting classes: {exc}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
    
    /// <summary>
    /// Bulk Soft Delete classes by a specific upstream domain. Used to avoid repeating functions.
    /// </summary>
    /// <param name="predicate">an anonymous function that allows the context to be filtered appropriately</param>
    /// <param name="transaction">(Optional) a transaction passed in from the parent to ensure ACID compliance</param>
    /// <returns>Boolean true on successful deletion</returns>
    private async Task SoftDeleteClasses(
            Expression<Func<Class, bool>> predicate, 
            IDbContextTransaction? transaction)
        {
            // check for existing transaction; if one does not exist, start a new one
            var commit = false; // flag used to determine if transaction should be committed
            if (transaction == null)
            {
                commit = true;
                transaction = await _context.Database.BeginTransactionAsync();
            }
            
            // search for records matching the passed-in predicate (filter) to be updated
            var classContext = _context.Classes
                .Where(c => c.DeletedAt == null)
                .Where(predicate);
            
            var classes = await classContext.ToListAsync();
            if (classes.Count == 0)
            {
                // return early if no records are to be deleted
                return;
            }
            
            var classIds = classes.Select(d => d.Id);
            
            // trigger downstream deletions
            var softDeleteTasks = new List<Func<Task<bool>>>
            {
                () => _edgeMappingBusiness.BulkSoftDeleteEdgeMappings(e => classIds.Contains(e.OriginId) || classIds.Contains(e.DestinationId)), 
                // unfortunately we need to assert that class id is not null here which looks really ugly
                () => _recordBusiness.BulkSoftDeleteRecords(r => classIds.Contains((long)r.ClassId), transaction), 
                () => _relationshipBusiness.BulkSoftDeleteRelationships(
                    r => classIds.Contains(r.OriginId) || classIds.Contains(r.DestinationId), transaction),
                // unfortunately we need to assert that class id is not null here which looks really ugly
                () => _recordMappingBusiness.BulkSoftDeleteRecordMappings(m => classIds.Contains((long)m.ClassId))
            };
            
            // loop through tasks and trigger downstream deletions
            foreach (var task in softDeleteTasks)
            {
                bool result = await task();
                if (!result)
                {
                    // rollback the transaction and throw an error
                    await transaction.RollbackAsync();
                    throw new DependencyDeletionException(
                        "An error occurred during the deletion of downstream class dependants.");
                }
            }

            // bulk update the results of the query to set the deleted_at date
            var updated = await classContext.ExecuteUpdateAsync(setters => setters
                .SetProperty(ds => ds.DeletedAt, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)));

            // if we found classes to update, but weren't successful in updating, throw an error
            if (updated == 0)
            {
                throw new DependencyDeletionException("An error occurred when deleting classes");
            }
            
            // save changes and commit transaction to close it
            await _context.SaveChangesAsync();
            if (commit)
            {
                await transaction.CommitAsync();
            }
        }
}