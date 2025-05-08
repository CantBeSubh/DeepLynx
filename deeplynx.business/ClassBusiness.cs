using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using deeplynx.helpers;
using Microsoft.EntityFrameworkCore;


namespace deeplynx.business;

public class ClassBusiness : IClassBusiness
{
    private readonly DeeplynxContext _context;

    public ClassBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Class>> GetAllClasses(long projectId)
    {
        return await _context.Classes
            .Where(c => c.ProjectId == projectId && c.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<Class> GetClass(long projectId, long classId)
    {
        return await _context.Classes
                   .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Id == classId && c.DeletedAt == null) ??
               throw new KeyNotFoundException("Class not found");
    }

    public async Task<Class> CreateClass(long projectId, ClassRequestDto dto)
    {
        var newClass = new Class
        {
            ProjectId = projectId,
            Name = dto.Name,
            Description = dto.Description,
            Uuid = dto.Uuid,
            CreatedAt =DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        };

        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();

        return newClass;
    }

    public async Task<Class> UpdateClass(long projectId, long classId, ClassRequestDto dto)
    {
        var updatedClass = await GetClass(projectId, classId);
        updatedClass.ProjectId = projectId;
        updatedClass.Name = dto.Name;
        updatedClass.Description = dto.Description;
        updatedClass.Uuid = dto.Uuid;
        updatedClass.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        updatedClass.ModifiedBy = null;  // TODO: Implement user ID here when JWT tokens are ready

        _context.Classes.Update(updatedClass);
        await _context.SaveChangesAsync();

        return updatedClass;
    }

    public async Task<bool> DeleteClass(long projectId, long classId)
    {
        var dbClass = await GetClass(projectId, classId);

        if (await IsClassInUse(classId))
            throw new InvalidOperationException("Cannot delete Class: it is still referenced by other entities.");

        dbClass.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        dbClass.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> IsClassInUse(long classId)
    {
        var inRecords = await _context.Records
            .AnyAsync(r => r.ClassId == classId && r.DeletedAt == null);

        var inRecordMappings = await _context.RecordMappings
            .AnyAsync(rp => rp.ClassId == classId && rp.DeletedAt == null);

        var inEdgeMappings = await _context.EdgeMappings
            .AnyAsync(ep => (ep.OriginId == classId || ep.DestinationId == classId) && ep.DeletedAt == null);

        var inRelationships = await _context.Relationships
            .AnyAsync(rel => (rel.OriginId == classId || rel.DestinationId == classId) && rel.DeletedAt == null);

        return inRecords || inRecordMappings || inEdgeMappings || inRelationships;
    }
}