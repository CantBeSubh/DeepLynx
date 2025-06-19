using System.Linq.Expressions;
using deeplynx.interfaces;
using deeplynx.models;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.business;

public class RelationshipBusiness: IRelationshipBusiness
{
      private readonly DeeplynxContext _context;
      private readonly IEdgeMappingBusiness _edgeMappingBusiness;
      private readonly IEdgeBusiness _edgeBusiness;

    public RelationshipBusiness(DeeplynxContext context, IEdgeMappingBusiness edgeMappingBusiness, IEdgeBusiness edgeBusiness)
    {
        _edgeMappingBusiness = edgeMappingBusiness;
        _edgeBusiness = edgeBusiness;
        _context = context;
    }
    public async Task<IEnumerable<RelationshipResponseDto>> GetAllRelationships(long projectId)
    {
        var rawData = await _context.Relationships
            .Where(r => r.ProjectId == projectId && r.ArchivedAt == null)
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
                r.ArchivedAt,
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
            .Where(r => r.ProjectId == projectId && r.Id == relationshipId && r.ArchivedAt == null)
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
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.ArchivedAt == null);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");
        }
        
        var orignClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.OriginId && c.ArchivedAt == null);
        if (orignClass == null)
        {
            throw new KeyNotFoundException($"Origin class with ID {dto.OriginId} not found.");
        }
        var destinationClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.DestinationId && c.ArchivedAt == null);
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
        if (relationship == null || relationship.ProjectId != projectId || relationship.ArchivedAt is not null)
        {
            throw new KeyNotFoundException($"Relationship with ID {relationshipId} not found.");
        }
        
        var orignClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.OriginId && c.ArchivedAt == null);
        if (orignClass == null)
        {
            throw new KeyNotFoundException($"Origin class with ID {dto.OriginId} not found.");
        }
        var destinationClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.DestinationId && c.ArchivedAt == null);
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

    public async Task<bool> DeleteRelationship(long projectId, long relationshipId, bool force = false)
    {
        var relationship = await _context.Relationships.FindAsync(relationshipId);
        
        if (relationship == null || relationship.ProjectId != projectId || relationship.ArchivedAt is not null)
        {
            throw new KeyNotFoundException($"Relationship with ID {relationshipId} not found.");
        }
        
        if (force)
        {
            // hard delete
            _context.Relationships.Remove(relationship);
            await _context.SaveChangesAsync();
        }
        else
        {
            try
            {
                var transaction = await _context.Database.BeginTransactionAsync();
                await SoftDeleteRelationships(r => r.Id == relationshipId, transaction);
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
    /// Bulk Soft Delete relationships by a specific upstream domain. Used to avoid repeating functions.
    /// </summary>
    /// <param name="predicate">an anonymous function that allows the context to be filtered appropriately</param>
    /// <param name="transaction">(Optional) a transaction passed in from the parent to ensure ACID compliance</param>
    /// <returns>Boolean true on successful deletion</returns>
    public async Task<bool> BulkSoftDeleteRelationships(
        Expression<Func<Relationship, bool>> predicate,
        IDbContextTransaction? transaction)
    {
        try
        {
            await SoftDeleteRelationships(predicate, transaction);
            return true;
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting relationships: {exc}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
    
    private async Task SoftDeleteRelationships(
            Expression<Func<Relationship, bool>> predicate,
            IDbContextTransaction? transaction)
        {
            // check for existing transaction; if one does not exist, start a new one
            var commit = false; // flag used to determine if transaction should be committed
            if (transaction == null)
            {
                commit = true;
                transaction = await _context.Database.BeginTransactionAsync();
            }

            // search for relationships matching the passed-in predicate (filter) to be updated
            var rContext = _context.Relationships
                .Where(d => d.ArchivedAt == null)
                .Where(predicate);

            var relationships = await rContext.ToListAsync();
            
            if (relationships.Count == 0)
            {
                // return early if no records are to be deleted
                return;
            }
            
            var relationshipIds = relationships.Select(d => d.Id);
            
            // trigger downstream deletions
            var softDeleteTasks = new List<Func<Task<bool>>>
            {
                () => _edgeMappingBusiness.BulkSoftDeleteEdgeMappings(e => relationshipIds.Contains(e.RelationshipId)),
                () => _edgeBusiness.BulkSoftDeleteEdges(e => e.RelationshipId.HasValue && relationshipIds.Contains(e.RelationshipId.Value))
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
                        "An error occurred during the deletion of downstream datasource dependants.");
                }
            }

            // bulk update the results of the query to set the archived_at date
            var updated = await rContext.ExecuteUpdateAsync(setters => setters
                .SetProperty(ds => ds.ArchivedAt, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)));

            // if we found relationships to update, but weren't successful in updating, throw an error
            if (updated == 0)
            {
                throw new DependencyDeletionException("An error occurred when deleting data sources.");
            }

            // save changes and commit transaction to close it
            await _context.SaveChangesAsync();
            if (commit)
            {
                await transaction.CommitAsync();
            }
        }
}