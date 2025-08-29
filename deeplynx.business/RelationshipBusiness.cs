using deeplynx.interfaces;
using deeplynx.models;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
    public async Task<List<RelationshipResponseDto>> GetAllRelationships(long projectId, bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
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
                r.LastUpdatedBy,
                r.LastUpdatedAt,
                r.IsArchived,
                r.OriginId,
                r.DestinationId
            })
            .ToListAsync();
        
        if (hideArchived)
        {
            relationships = relationships.Where(r => !r.IsArchived).ToList();
        }
        
        // Manual mapping to Relationship objects to match return type without getting infite loop on Origin or Destination
        return relationships.Select(r => new RelationshipResponseDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Uuid = r.Uuid,
            ProjectId = r.ProjectId,
            OriginId = r.OriginId,
            DestinationId = r.DestinationId,
            LastUpdatedBy = r.LastUpdatedBy,
            LastUpdatedAt = r.LastUpdatedAt,
            IsArchived = r.IsArchived,
        }).ToList();
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
        var relationship = await _context.Relationships
            .Where(r => r.ProjectId == projectId && r.Id == relationshipId)
            .Include(r => r.Origin)
            .Include(r => r.Destination)
            .FirstOrDefaultAsync();

        if (relationship == null)
        {
            throw new KeyNotFoundException($"Relationship with ID {relationshipId} not found.");
        }
        
        if (hideArchived && relationship.IsArchived)
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
            LastUpdatedBy = relationship.LastUpdatedBy,
            LastUpdatedAt = relationship.LastUpdatedAt,
            IsArchived = relationship.IsArchived,
            OriginId = relationship.OriginId,
            DestinationId = relationship.DestinationId,
        };
    }
    
    /// <summary>
    /// Creates a new relationship based on the data transfer object supplied.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs</param>
    /// <param name="dto">A data transfer object with details on the new relationship to be created.</param>
    /// <returns>The new relationship which was just created.</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship or origin/destination classes not found</exception>
    public async Task<RelationshipResponseDto> CreateRelationship(long projectId, CreateRelationshipRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        ValidationHelper.ValidateModel(dto);
        
        var existingRelationship = await _context.Relationships.FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Name == dto.Name);
        if (existingRelationship != null)
        {
            throw new Exception($"Relationship for project {projectId} with name {dto.Name} already exists");
        }
        
        if (dto.OriginId != null)
        { 
            var originClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.OriginId && !c.IsArchived);
            if (originClass == null)
            {
                throw new KeyNotFoundException($"Origin class with ID {dto.OriginId} not found.");
            }
        }

        if (dto.DestinationId != null)
        {
            var destinationClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == dto.DestinationId && c.IsArchived == null);
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
            LastUpdatedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null , // TODO: Implement user ID here when JWT tokens are ready
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
            LastUpdatedBy = relationship.LastUpdatedBy,
            LastUpdatedAt = relationship.LastUpdatedAt,
            IsArchived = relationship.IsArchived,
            OriginId = relationship.OriginId,
            DestinationId = relationship.DestinationId
        };
    }

    /// <summary>
    /// Creates a new relationship based on the data transfer object supplied.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs</param>
    /// <param name="relationships">A list of relationship data transfer objects with details on the new relationship to be created.</param>
    /// <returns>The new relationship which was just created.</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship or origin/destination classes not found</exception>
    public async Task<List<RelationshipResponseDto>> BulkCreateRelationships(
        long projectId, 
        List<CreateRelationshipRequestDto> relationships)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        
        // Bulk insert into relationships; if there is a name collision, update the description and uuid if present
        var sql = @"
            INSERT INTO deeplynx.relationships (project_id, name, description, uuid, created_at)
            VALUES {0}
            ON CONFLICT (project_id, name) DO UPDATE SET
                description = COALESCE(EXCLUDED.description, relationships.description),
                uuid = COALESCE(EXCLUDED.uuid, relationships.uuid),
                last_updated_at = @now
            RETURNING *;
        ";
        
        // establish "constant" parameters
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@projectId", projectId),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
        };
        
        // establish "dynamic" parameters (new for each dto in the list)
        parameters.AddRange(relationships.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@p{i}_name", dto.Name),
            new NpgsqlParameter($"@p{i}_desc", (object?)dto.Description ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_uuid", (object?)dto.Uuid ?? DBNull.Value),
        }));
        
        // stringify the params and comma separate them
        var valueTuples = string.Join(", ", relationships.Select((dto, i) =>
            $"(@projectId, @p{i}_name, @p{i}_desc, @p{i}_uuid, @now, false, NULL)"));
        
        // put everything together and execute the query
        sql = string.Format(sql, valueTuples);

        // returns the resulting upserted relationships
        return await _context.Database
            .SqlQueryRaw<RelationshipResponseDto>(sql, parameters.ToArray())
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing relationship by its ID.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the relationship belongs.</param>
    /// <param name="relationshipId">The ID of the relationship to update.</param>
    /// <param name="dto">The relationship request data transfer object containing updated relationship details.</param>
    /// <returns>The updated relationship response DTO with its details</returns>
    /// <exception cref="KeyNotFoundException">Returned if relationship or origin/destination classes not found</exception>
    public async Task<RelationshipResponseDto> UpdateRelationship(long projectId, long relationshipId, UpdateRelationshipRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var relationship = await _context.Relationships.FindAsync(relationshipId);
        if (relationship == null || relationship.ProjectId != projectId || relationship.IsArchived)
        {
            throw new KeyNotFoundException($"Relationship with ID {relationshipId} not found.");
        }

        var orignClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == (dto.OriginId ?? relationship.OriginId) && !c.IsArchived);
        if (orignClass == null)
        {
            throw new KeyNotFoundException($"Origin class with ID {dto.OriginId} not found.");
        }

        var destinationClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == (dto.DestinationId ?? relationship.DestinationId) && !c.IsArchived);
        if (destinationClass == null)
        {
            throw new KeyNotFoundException($"Destination class with ID {dto.DestinationId} not found.");
        }
        
        relationship.Name = dto.Name ?? relationship.Name;
        relationship.Description = dto.Description ?? relationship.Description;
        relationship.Uuid = dto.Uuid ?? relationship.Uuid;
        relationship.OriginId = dto.OriginId ?? relationship.OriginId;
        relationship.DestinationId = dto.DestinationId ?? relationship.DestinationId;
        relationship.LastUpdatedAt =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        relationship.LastUpdatedBy = null;  // TODO: Implement user ID here when JWT tokens are ready;
        _context.Relationships.Update(relationship);
        await _context.SaveChangesAsync();
        return new RelationshipResponseDto
        {
            Id = relationship.Id,
            Name = relationship.Name,
            Description = relationship.Description,
            Uuid = relationship.Uuid,
            ProjectId = relationship.ProjectId,
            LastUpdatedBy = relationship.LastUpdatedBy,
            LastUpdatedAt = relationship.LastUpdatedAt,
            IsArchived = relationship.IsArchived,
            OriginId = relationship.OriginId,
            DestinationId = relationship.DestinationId
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var relationship = await _context.Relationships.FindAsync(relationshipId);

        if (relationship == null || relationship.ProjectId != projectId || relationship.IsArchived)
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
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var relationship = await _context.Relationships.FindAsync(relationshipId);

        if (relationship == null || relationship.ProjectId != projectId || !relationship.IsArchived)
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
    /// Validates that all provided relationship names exist in the database for the specified project.
    /// Used by MetadataBusiness to enforce ontologyMutable settings.
    /// </summary>
    /// <param name="projectId">The project ID to search within</param>
    /// <param name="relationshipNames">List of relationship names to validate</param>
    /// <returns>List of relationships that were found</returns>
    /// <exception cref="KeyNotFoundException">Thrown if one or more relationship names not found</exception>
    /// <exception cref="ArgumentException">Thrown if relationshipNames list is null or empty</exception>
    public async Task<List<RelationshipResponseDto>> GetRelationshipsByName(long projectId, List<string> relationshipNames)
    {
        if (relationshipNames == null || !relationshipNames.Any())
            throw new ArgumentException("Relationship names list cannot be null or empty", nameof(relationshipNames));

        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);

        var cleanRelationshipNames = relationshipNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList();

        if (!cleanRelationshipNames.Any())
            throw new ArgumentException("No valid relationship names provided", nameof(relationshipNames));
    
        var existingRelationships = await _context.Relationships
            .Where(r => r.ProjectId == projectId 
                        && r.IsArchived == false
                        && cleanRelationshipNames.Contains(r.Name))
            .Include(r => r.Origin)
            .Include(r => r.Destination)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.Uuid,
                r.ProjectId,
                r.LastUpdatedBy,
                r.LastUpdatedAt,
                r.IsArchived,
                r.OriginId,
                r.DestinationId
            })
            .ToListAsync();

        // Check for missing relationships
        var foundRelationshipNames = existingRelationships.Select(r => r.Name).ToHashSet();
        var missingRelationshipNames = cleanRelationshipNames.Where(name => !foundRelationshipNames.Contains(name)).ToList();

        if (missingRelationshipNames.Any())
        {
            throw new KeyNotFoundException(
                $"Relationships not found with names: {string.Join(", ", missingRelationshipNames)}");
        }

        // Convert to DTOs (manual mapping to avoid infinite loop with Origin/Destination)
        return existingRelationships.Select(r => new RelationshipResponseDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Uuid = r.Uuid,
            ProjectId = r.ProjectId,
            LastUpdatedBy = r.LastUpdatedBy,
            LastUpdatedAt = r.LastUpdatedAt,
            OriginId = r.OriginId,
            DestinationId = r.DestinationId,
            IsArchived = r.IsArchived
        }).ToList();
    }
}