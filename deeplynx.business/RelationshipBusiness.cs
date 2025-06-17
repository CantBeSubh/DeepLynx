using deeplynx.interfaces;
using deeplynx.models;
using deeplynx.datalayer.Models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class RelationshipBusiness: IRelationshipBusiness
{
      private readonly DeeplynxContext _context;

    public RelationshipBusiness(DeeplynxContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<RelationshipResponseDto>> GetAllRelationships(long projectId)
    {
        var rawData = await _context.Relationships
            .Where(r => r.ProjectId == projectId && r.DeletedAt == null)
            .Include(r => r.Origin)
            .Include(r => r.Destination)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.Uuid,
                r.ProjectId,
                r.CreatedBy,
                r.CreatedAt,
                r.ModifiedBy,
                r.ModifiedAt,
                r.DeletedAt,
                r.OriginId,
                r.DestinationId,
                Origin = r.Origin == null ? null : new ClassRelationshipRespDto { Id = r.Origin.Id, Name = r.Origin.Name },
                Destination = r.Destination == null ? null : new ClassRelationshipRespDto { Id = r.Destination.Id, Name = r.Destination.Name }
            })
            .ToListAsync();
        // Manual mapping to Relationship objects to match return type without getting infite loop on Origin or Destination
        var result = rawData.Select(r => new RelationshipResponseDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Uuid = r.Uuid,
            ProjectId = r.ProjectId,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt,
            ModifiedBy = r.ModifiedBy,
            ModifiedAt = r.ModifiedAt,
            OriginId = r.OriginId,
            DestinationId = r.DestinationId,
            Origin = r.Origin,
            Destination = r.Destination
        });

        return result;
    }
    public async Task<RelationshipResponseDto> GetRelationship(long projectId, long relationshipId)
    {
        var relationship = await _context.Relationships
            .Where(r => r.ProjectId == projectId && r.Id == relationshipId && r.DeletedAt == null)
            .Include(r => r.Origin)
            .Include(r => r.Destination)
            .FirstOrDefaultAsync();

        if (relationship == null)
        {
            throw new KeyNotFoundException($"Relationship with ID {relationshipId} not found.");
        }

        return new RelationshipResponseDto
        {
            Id = relationship.Id,
            Name = relationship.Name,
            Description = relationship.Description,
            Uuid = relationship.Uuid,
            ProjectId = relationship.ProjectId,
            CreatedBy = relationship.CreatedBy,
            CreatedAt = relationship.CreatedAt,
            ModifiedBy = relationship.ModifiedBy,
            ModifiedAt = relationship.ModifiedAt,
            OriginId = relationship.OriginId,
            DestinationId = relationship.DestinationId,
            Origin = relationship.Origin == null ? null : new ClassRelationshipRespDto { Id = relationship.Origin.Id, Name = relationship.Origin.Name },
            Destination = relationship.Destination == null ? null : new ClassRelationshipRespDto { Id = relationship.Destination.Id, Name = relationship.Destination.Name }
        };
    }
    public async Task<RelationshipResponseDto> CreateRelationship(long projectId, RelationshipRequestDto dto)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.DeletedAt == null);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");
        }
        
        var orignClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.OriginId && c.DeletedAt == null);
        if (orignClass == null)
        {
            throw new KeyNotFoundException($"Origin class with ID {dto.OriginId} not found.");
        }
        var destinationClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.DestinationId && c.DeletedAt == null);
        if (destinationClass == null)
        {
            throw new KeyNotFoundException($"Destination class with ID {dto.DestinationId} not found.");
        }
        
        var relationship = new Relationship
        {
            Name = dto.Name,
            Description = dto.Description,
            Uuid = dto.Uuid,
            OriginId = dto.OriginId,
            DestinationId = dto.DestinationId,
            ProjectId = projectId,
            CreatedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            ModifiedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null , // TODO: Implement user ID here when JWT tokens are ready
            ModifiedBy =null  // TODO: Implement user ID here when JWT tokens are ready
        };

        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();
        await _context.Entry(relationship)
            .Reference(r => r.Origin)
            .LoadAsync();
        await _context.Entry(relationship)
            .Reference(r => r.Destination)
            .LoadAsync();
        return new RelationshipResponseDto
        {
            Id = relationship.Id,
            Name = relationship.Name,
            Description = relationship.Description,
            Uuid = relationship.Uuid,
            ProjectId = relationship.ProjectId,
            CreatedBy = relationship.CreatedBy,
            CreatedAt = relationship.CreatedAt,
            ModifiedBy = relationship.ModifiedBy,
            ModifiedAt = relationship.ModifiedAt,
            OriginId = relationship.OriginId,
            DestinationId = relationship.DestinationId,
            Origin = relationship.Origin == null
                ? null
                : new ClassRelationshipRespDto { Id = relationship.Origin.Id, Name = relationship.Origin.Name },
            Destination = relationship.Destination == null
                ? null
                : new ClassRelationshipRespDto { Id = relationship.Destination.Id, Name = relationship.Destination.Name }
        };
    }

    public async Task<RelationshipResponseDto> UpdateRelationship(long projectId, long relationshipId, RelationshipRequestDto dto)
    {
        var relationship = await _context.Relationships.FindAsync(relationshipId);
        if (relationship == null || relationship.ProjectId != projectId || relationship.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Relationship with ID {relationshipId} not found.");
        }
        
        var orignClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.OriginId && c.DeletedAt == null);
        if (orignClass == null)
        {
            throw new KeyNotFoundException($"Origin class with ID {dto.OriginId} not found.");
        }
        var destinationClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.DestinationId && c.DeletedAt == null);
        if (destinationClass == null)
        {
            throw new KeyNotFoundException($"Destination class with ID {dto.DestinationId} not found.");
        }
        
        relationship.Name = dto.Name;
        relationship.Description = dto.Description;
        relationship.Uuid = dto.Uuid;
        relationship.OriginId = dto.OriginId;
        relationship.DestinationId = dto.DestinationId;
        relationship.ModifiedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        relationship.ModifiedBy = null;  // TODO: Implement user ID here when JWT tokens are ready;
        _context.Relationships.Update(relationship);
        await _context.SaveChangesAsync();
        return new RelationshipResponseDto
        {
            Id = relationship.Id,
            Name = relationship.Name,
            Description = relationship.Description,
            Uuid = relationship.Uuid,
            ProjectId = relationship.ProjectId,
            CreatedBy = relationship.CreatedBy,
            CreatedAt = relationship.CreatedAt,
            ModifiedBy = relationship.ModifiedBy,
            ModifiedAt = relationship.ModifiedAt,
            OriginId = relationship.OriginId,
            DestinationId = relationship.DestinationId,
            Origin = relationship.Origin == null
                ? null
                : new ClassRelationshipRespDto { Id = relationship.Origin.Id, Name = relationship.Origin.Name },
            Destination = relationship.Destination == null
                ? null
                : new ClassRelationshipRespDto { Id = relationship.Destination.Id, Name = relationship.Destination.Name }
        };
    }

    public async Task<bool> DeleteRelationship(long projectId, long relationshipId)
    {
        var relationship = await _context.Relationships.FindAsync(relationshipId);
        if (relationship == null || relationship.ProjectId != projectId || relationship.DeletedAt is not null)
        {
            throw new KeyNotFoundException($"Relationship with ID {relationshipId} not found.");
        }
        relationship.DeletedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        relationship.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Bulk Soft Delete relationships by a specific upstream domain. Used to avoid repeating functions.
    /// </summary>
    /// <param name="domainType">The type of domain which is calling this function</param>
    /// <param name="domainId">The ID of the upstream domain calling this function</param>
    /// <returns>Boolean true on successful deletion</returns>
    public async Task<bool> BulkSoftDeleteRelationships(string domainType, long domainId)
    {
        try
        {
            var relationshipQuery = _context.Relationships.Where(r => r.DeletedAt == null);

            if (domainType == "project")
            {
                relationshipQuery = relationshipQuery.Where(r => r.ProjectId == domainId);
            }
                    
            var relationships = await relationshipQuery.ToListAsync();
                
            foreach (var r in relationships)
            {
                r.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }
                
            await _context.SaveChangesAsync();
            return true;
                
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting relationships for domain {domainType} with id {domainId}: {exc}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
}