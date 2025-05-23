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
    private async Task<long> ResolveClassIdentifier(string input)
    {
        if (long.TryParse(input, out var id))
        {
            var dbClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
            return dbClass?.Id ?? throw new KeyNotFoundException($"Class with ID {id} not found.");
        }

        var classByUuid = await _context.Classes
            .Where(c => c.Uuid == input && c.DeletedAt == null)
            .Select(c => (long?)c.Id)
            .FirstOrDefaultAsync();

        return classByUuid ?? throw new KeyNotFoundException($"Class with UUID ‘{input}’ not found.");
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
        if(string.IsNullOrWhiteSpace(dto.OriginClass))
            throw new KeyNotFoundException($"Origin class is empty.");
        if(string.IsNullOrWhiteSpace(dto.DestinationClass))
            throw new KeyNotFoundException($"Destination class is empty.");
        var originId = await ResolveClassIdentifier(dto.OriginClass);
        var destinationId = await ResolveClassIdentifier(dto.DestinationClass);

        var relationship = new Relationship
        {
            Name = dto.Name,
            Description = dto.Description,
            Uuid = dto.Uuid,
            OriginId = originId,
            DestinationId = destinationId,
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
        
        relationship.Name = dto.Name;
        relationship.Description = dto.Description;
        relationship.Uuid = dto.Uuid;
        relationship.OriginId = await ResolveClassIdentifier(dto.OriginClass);
        relationship.DestinationId = await ResolveClassIdentifier(dto.DestinationClass);
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
}