using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class ClassBusiness : IClassBusiness
{
    private readonly DeeplynxContext _context;

    public ClassBusiness(DeeplynxContext context)
    {
        _context = context;
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
        updatedClass.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);;
        
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

    public async Task<bool> DeleteClass(long projectId, long classId)
    {
        var dbClass = await _context.Classes.FindAsync(classId);

        if (await IsClassInUse(classId))
            throw new InvalidOperationException("Cannot delete Class: it is still referenced by other entities.");
        else if (dbClass == null || dbClass.ProjectId != projectId || dbClass.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Class with id {classId} not found");
        }
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