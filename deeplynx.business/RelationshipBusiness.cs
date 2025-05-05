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

    public async Task<IEnumerable<Relationship>> GetAllRelationships(long projectId)
    {
        return await _context.Relationships
            .Where(r => r.ProjectId == projectId && r.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<Relationship> GetRelationship(long projectId, long relationshipId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Id == relationshipId && r.DeletedAt == null)
            ?? throw new KeyNotFoundException("Relationship not found.");
    }

    public async Task<Relationship> CreateRelationship(long projectId, RelationshipRequestDto dto)
    {
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
        return relationship;
    }

    public async Task<Relationship> UpdateRelationship(long projectId, long relationshipId, RelationshipRequestDto dto)
    {
        var relationship = await GetRelationship(projectId, relationshipId);
        relationship.Name = dto.Name;
        relationship.Description = dto.Description;
        relationship.Uuid = dto.Uuid;
        relationship.OriginId = await ResolveClassIdentifier(dto.OriginClass);
        relationship.DestinationId = await ResolveClassIdentifier(dto.DestinationClass);
        relationship.ModifiedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        relationship.ModifiedBy = null;  // TODO: Implement user ID here when JWT tokens are ready;

        await _context.SaveChangesAsync();
        return relationship;
    }

    public async Task<bool> DeleteRelationship(long projectId, long relationshipId)
    {
        var relationship = await GetRelationship(projectId, relationshipId);
        relationship.DeletedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        relationship.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        await _context.SaveChangesAsync();
        return true;
    }
}