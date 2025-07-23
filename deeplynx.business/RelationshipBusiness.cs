using System.Linq.Expressions;
using deeplynx.interfaces;
using deeplynx.models;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.business;

public class RelationshipBusiness: IRelationshipBusiness
{
      private readonly DeeplynxContext _context;
      private readonly IEdgeMappingBusiness _edgeMappingBusiness;
      private readonly IEdgeBusiness _edgeBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationshipBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the relationship operations.</param>
    /// <param name="edgeMappingBusiness">Passed in context of edge mapping objects.</param>
    /// <param name="edgeBusiness">Passed in context of edge objects.</param>
    public RelationshipBusiness(DeeplynxContext context, IEdgeMappingBusiness edgeMappingBusiness, IEdgeBusiness edgeBusiness)
    {
        _edgeMappingBusiness = edgeMappingBusiness;
        _edgeBusiness = edgeBusiness;
        _context = context;
    }
    
    /// <summary>
    /// Retrieves all relationships
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs.</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived relationships from the result</param>
    /// <returns>A list of relationships</returns>
    public async Task<IEnumerable<RelationshipResponseDto>> GetAllRelationships(long projectId, bool hideArchived)
    {
        DoesProjectExist(projectId, hideArchived);
        var relationships = await _context.Relationships
            .Where(r => r.ProjectId == projectId)
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
                Origin = r.Origin == null ? null : new ClassRelationshipResponseDto { Id = r.Origin.Id, Name = r.Origin.Name },
                Destination = r.Destination == null ? null : new ClassRelationshipResponseDto { Id = r.Destination.Id, Name = r.Destination.Name }
            })
            .ToListAsync();
        
        if (hideArchived)
        {
            relationships = relationships.Where(r => r.ArchivedAt == null).ToList();
        }
        
        // Manual mapping to Relationship objects to match return type without getting infite loop on Origin or Destination
        return relationships.Select(r => new RelationshipResponseDto
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
            Destination = r.Destination,
            ArchivedAt = r.ArchivedAt,
        });
    }
    /// <summary>
    /// Retrieves a specific relationship by ID
    /// </summary>
    /// <param name="projectId">The ID by which to retrieve the class</param>
    /// <param name="relationshipId">The ID of the relationship to retrieve</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result</param>
    /// <returns>The given relationship to return</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship not found or is archived</exception>
    public async Task<RelationshipResponseDto> GetRelationship(long projectId, long relationshipId, bool hideArchived)
    {
        DoesProjectExist(projectId, hideArchived);
        var relationship = await _context.Relationships
            .Where(r => r.ProjectId == projectId && r.Id == relationshipId)
            .Include(r => r.Origin)
            .Include(r => r.Destination)
            .FirstOrDefaultAsync();

        if (relationship == null)
        {
            throw new KeyNotFoundException($"Relationship with ID {relationshipId} not found.");
        }
        
        if (hideArchived && relationship.ArchivedAt != null)
        {
            throw new KeyNotFoundException($"Relationship with id {relationshipId} is archived");
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
            Origin = relationship.Origin == null ? null : new ClassRelationshipResponseDto { Id = relationship.Origin.Id, Name = relationship.Origin.Name },
            Destination = relationship.Destination == null ? null : new ClassRelationshipResponseDto { Id = relationship.Destination.Id, Name = relationship.Destination.Name },
            ArchivedAt = relationship.ArchivedAt,
        };
    }
    
    /// <summary>
    /// Creates a new relationship based on the data transfer object supplied.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs</param>
    /// <param name="dto">A data transfer object with details on the new relationship to be created.</param>
    /// <returns>The new relationship which was just created.</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship or origin/destination classes not found</exception>
    public async Task<RelationshipResponseDto> CreateRelationship(long projectId, RelationshipRequestDto dto)
    {
        DoesProjectExist(projectId);
        ValidationHelper.ValidateModel(dto);
        
        var existingRelationship = await _context.Relationships.FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Name == dto.Name);
        if (existingRelationship != null)
        {
            throw new Exception($"Relationship for project {projectId} with name {dto.Name} already exists");
        }
        
        if (dto.OriginId != null)
        { 
            var originClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.OriginId && c.ArchivedAt == null);
            if (originClass == null)
            {
                throw new KeyNotFoundException($"Origin class with ID {dto.OriginId} not found.");
            }
        }

        if (dto.DestinationId != null)
        {
            var destinationClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.DestinationId && c.ArchivedAt == null);
            if (destinationClass == null)
            {
                throw new KeyNotFoundException($"Destination class with ID {dto.DestinationId} not found.");
            }
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

        if (dto.OriginId != null)
        {
            await _context.Entry(relationship)
                .Reference(r => r.Origin)
                .LoadAsync();
        }

        if (dto.DestinationId != null)
        {
            await _context.Entry(relationship)
                .Reference(r => r.Destination)
                .LoadAsync();
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
            Origin = relationship.Origin == null
                ? null
                : new ClassRelationshipResponseDto { Id = relationship.Origin.Id, Name = relationship.Origin.Name },
            Destination = relationship.Destination == null
                ? null
                : new ClassRelationshipResponseDto { Id = relationship.Destination.Id, Name = relationship.Destination.Name }
        };
    }

    /// <summary>
    /// Creates a new relationship based on the data transfer object supplied.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs</param>
    /// <param name="bulkRelationshipRequestDto">A data transfer object with details on the new relationship to be created.</param>
    /// <returns>The new relationship which was just created.</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship or origin/destination classes not found</exception>
    public async Task<List<RelationshipResponseDto>> BulkCreateRelationships(long projectId, List<RelationshipRequestDto> relationshipRequestDtos)
    {
        DoesProjectExist(projectId);
        
        var relationshipEntities = new List<Relationship>();
        foreach (var relationshipRequestDto in relationshipRequestDtos)
        {
            ValidationHelper.ValidateModel(relationshipRequestDto);
            
            if (relationshipRequestDto.OriginId != null)
            { 
                var originClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == relationshipRequestDto.OriginId && c.ArchivedAt == null);
                if (originClass == null)
                {
                    throw new KeyNotFoundException($"Origin class with ID {relationshipRequestDto.OriginId} not found.");
                }
            }

            if (relationshipRequestDto.DestinationId != null)
            {
                var destinationClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == relationshipRequestDto.DestinationId && c.ArchivedAt == null);
                if (destinationClass == null)
                {
                    throw new KeyNotFoundException($"Destination class with ID {relationshipRequestDto.DestinationId} not found.");
                }
            }
            
            var relationship = new Relationship
            {
                Name = relationshipRequestDto.Name,
                Description = relationshipRequestDto.Description,
                Uuid = relationshipRequestDto.Uuid,
                OriginId = relationshipRequestDto.OriginId,
                DestinationId = relationshipRequestDto.DestinationId,
                ProjectId = projectId,
                CreatedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ModifiedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null , // TODO: Implement user ID here when JWT tokens are ready
                ModifiedBy =null  // TODO: Implement user ID here when JWT tokens are ready
            };
            relationshipEntities.Add(relationship);
        }

        await _context.Relationships.AddRangeAsync(relationshipEntities);
        await _context.SaveChangesAsync();

        var relationshipResponseDtos = new List<RelationshipResponseDto>();
        foreach (var relationship in relationshipEntities)
        {
            if (relationship.OriginId != null)
            {
                await _context.Entry(relationship)
                    .Reference(r => r.Origin)
                    .LoadAsync();
            }

            if (relationship.DestinationId != null)
            {
                await _context.Entry(relationship)
                    .Reference(r => r.Destination)
                    .LoadAsync();
            }
            
            var relationshipResponseDto = new RelationshipResponseDto
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
                    : new ClassRelationshipResponseDto { Id = relationship.Origin.Id, Name = relationship.Origin.Name },
                Destination = relationship.Destination == null
                    ? null
                    : new ClassRelationshipResponseDto { Id = relationship.Destination.Id, Name = relationship.Destination.Name }
            };
            relationshipResponseDtos.Add(relationshipResponseDto);
        }

        return relationshipResponseDtos;
    }

    /// <summary>
    /// Updates an existing relationship by its ID.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs.</param>
    /// <param name="relationshipId">The ID of the relationship to update.</param>
    /// <param name="dto">The relationship request data transfer object containing updated relationship details.</param>
    /// <returns>The updated relationship response DTO with its details</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship or origin/destination classes not found</exception>
    public async Task<RelationshipResponseDto> UpdateRelationship(long projectId, long relationshipId, RelationshipRequestDto dto)
    {
        DoesProjectExist(projectId);
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
                : new ClassRelationshipResponseDto { Id = relationship.Origin.Id, Name = relationship.Origin.Name },
            Destination = relationship.Destination == null
                ? null
                : new ClassRelationshipResponseDto { Id = relationship.Destination.Id, Name = relationship.Destination.Name }
        };
    }

    /// <summary>
    /// Delete a specific relationship by ID
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs.</param>
    /// <param name="relationshipId">The ID of the relationship to delete</param>
    /// <returns>Boolean true on successful deletion</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship not found</exception>
    public async Task<bool> DeleteRelationship(long projectId, long relationshipId)
    {
        DoesProjectExist(projectId);
        var relationship = await _context.Relationships.FindAsync(relationshipId);

        if (relationship == null || relationship.ProjectId != projectId)
            throw new KeyNotFoundException($"Relationship with id {relationshipId} not found");

        _context.Relationships.Remove(relationship);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Archive a specific relationship by ID
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs.</param>
    /// <param name="relationshipId">The ID of the relationship to archive</param>
    /// <returns>Boolean true on successful archive</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship not found</exception>
    public async Task<bool> ArchiveRelationship(long projectId, long relationshipId)
    {
        DoesProjectExist(projectId);
        var relationship = await _context.Relationships.FindAsync(relationshipId);

        if (relationship == null || relationship.ProjectId != projectId || relationship.ArchivedAt is not null)
            throw new KeyNotFoundException($"Relationship with id {relationshipId} not found");

        // set archivedAt timestamp
        var archivedAt = DateTime.UtcNow;
        
        // run archive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the archive relationship procedure, which archives this relationship
                // and all child objects with relationship_id as a foreign key
                var archived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.archive_relationship({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)", relationshipId, archivedAt);

                if (archived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException($"unable to archive relationship {relationshipId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException($"unable to archive relationship {relationshipId} or its downstream dependents: {exc}");
            }
        }
    }
    
    /// <summary>
    /// Unarchive a specific relationship by ID
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs.</param>
    /// <param name="relationshipId">The ID of the relationship to unarchive</param>
    /// <returns>Boolean true on successful unarchive action</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship not found</exception>
    public async Task<bool> UnarchiveRelationship(long projectId, long relationshipId)
    {
        DoesProjectExist(projectId);
        var relationship = await _context.Relationships.FindAsync(relationshipId);

        if (relationship == null || relationship.ProjectId != projectId || relationship.ArchivedAt is null)
            throw new KeyNotFoundException($"Relationship with id {relationshipId} not found or is not archived.");
        
        // run unarchive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the unarchive relationship procedure, which unarchives this relationship
                // and all child objects with relationship_id as a foreign key
                var unarchived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.unarchive_relationship({0}::INTEGER)", relationshipId);

                if (unarchived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException($"unable to unarchive relationship {relationshipId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException($"unable to unarchive relationship {relationshipId} or its downstream dependents: {exc}");
            }
        }
    }
    
    /// <summary>
    /// Determine if project exists
    /// </summary>
    /// <param name="projectId">The ID of the project we are searching for</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
    /// <returns>Throws error if project does not exist</returns>
    private void DoesProjectExist(long projectId, bool hideArchived = true)
    {
        var project = hideArchived ? _context.Projects.Any(p => p.Id == projectId && p.ArchivedAt == null) 
            : _context.Projects.Any(p => p.Id == projectId);
        if (!project)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }
    }
    
    
}