using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using deeplynx.helpers;
using Npgsql;
using System.Text.Json;
using deeplynx.helpers.Context;

namespace deeplynx.business;

public class ClassBusiness : IClassBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IRecordBusiness _recordBusiness;
    private readonly IRelationshipBusiness _relationshipBusiness;
    private readonly IEventBusiness _eventBusiness;
    private readonly ICacheBusiness _cacheBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for class operations</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    /// <param name="edgeMappingBusiness">Passed in context of edge mapping objects</param>
    /// <param name="recordBusiness">Passed in context of record objects</param>
    /// <param name="relationshipBusiness">Passed in context of relationship objects</param>
    /// <param name="eventBusiness">Used for logging events during create, update, and delete Operations.</param>
    public ClassBusiness(
        DeeplynxContext context,
        ICacheBusiness? cacheBusiness,
        IRecordBusiness recordBusiness,
        IRelationshipBusiness relationshipBusiness,
        IEventBusiness eventBusiness
    )
    {
        _context = context;
        _recordBusiness = recordBusiness;
        _relationshipBusiness = relationshipBusiness;
        _eventBusiness = eventBusiness;
        _cacheBusiness = cacheBusiness;
    }

    /// <summary>
    /// Retrieves all classes
    /// </summary>
    /// <param name="projectId">The ID of the project to which the class belongs</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result</param>
    /// <returns>A list of classes</returns>
    public async Task<List<ClassResponseDto>> GetAllClasses(long projectId, bool hideArchived)
    {
       await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);
       var classes = await _context.Classes
            .Where(c => c.ProjectId == projectId).ToListAsync();
        
        if (hideArchived)
        {
            classes = classes.Where(c => !c.IsArchived).ToList();
        }
        
        return classes 
            .Select(c => new ClassResponseDto()
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Uuid = c.Uuid,
                ProjectId = c.ProjectId,
                LastUpdatedAt = c.LastUpdatedAt,
                LastUpdatedBy = c.LastUpdatedBy,
                IsArchived = c.IsArchived,

            }).ToList();
    }


    /// <summary>
    /// Retrieves a specific class by ID
    /// </summary>
    /// <param name="projectId">The ID by which to retrieve the class</param>
    /// <param name="classId">The ID of the class to retrieve</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result</param>
    /// <returns>The given class to return</returns>
    /// <exception cref="KeyNotFoundException">Returned if class not found or is archived</exception>
    public async Task<ClassResponseDto> GetClass(long projectId, long classId, bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId,  _cacheBusiness, hideArchived);
        var newClass = await _context.Classes
            .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Id == classId);
        if (newClass == null)
        {
            throw new KeyNotFoundException($"Class with id {classId} not found");
        }
        
        if (hideArchived &&  newClass.IsArchived)
        {
            throw new KeyNotFoundException($"Class with id {classId} is archived");
        }

        return new ClassResponseDto
        {
            Id = newClass.Id,
            Name = newClass.Name,
            Description = newClass.Description,
            Uuid = newClass.Uuid,
            ProjectId = newClass.ProjectId,
            LastUpdatedAt = newClass.LastUpdatedAt,
            LastUpdatedBy = newClass.LastUpdatedBy,
            IsArchived = newClass.IsArchived

        };
    }

    /// <summary>
    /// Creates a new class based on the data transfer object supplied.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">The ID of the project to which the class belongs</param>
    /// <param name="dto">A data transfer object with details on the new class to be created.</param>
    /// <returns>The new class which was just created.</returns>
    /// <exception cref="KeyNotFoundException">Returned if class not found</exception>
    /// <exception cref="Exception">Returned if class already exists</exception>
    public async Task<ClassResponseDto> CreateClass(long currentUserId, long projectId, CreateClassRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId,  _cacheBusiness);
        ValidationHelper.ValidateModel(dto);

        var newClass = new Class
        {
            ProjectId = projectId,
            Name = dto.Name,
            Description = dto.Description,
            Uuid = dto.Uuid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = currentUserId,
            IsArchived = false
        };

        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();
        
        // log event with class create details
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "create",
            EntityType = "class",
            EntityId = newClass.Id,
            EntityName = newClass.Name,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new {newClass.Name}),
        });

        return new ClassResponseDto
        {
            Id = newClass.Id,
            Name = newClass.Name,
            Description = newClass.Description,
            Uuid = newClass.Uuid,
            ProjectId = newClass.ProjectId,
            LastUpdatedAt = newClass.LastUpdatedAt,
            LastUpdatedBy = newClass.LastUpdatedBy,
            IsArchived = newClass.IsArchived

        };
    }
    
    /// <summary>
    /// Creates a new classes based on the data transfer object supplied.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">The ID of the project to which the class belongs</param>
    /// <param name="classes">A list of class data transfer object with details on the new class to be created.</param>
    /// <returns>The new class which was just created.</returns>
    /// <exception cref="Exception">Returned if class already exists</exception>
    public async Task<List<ClassResponseDto>> BulkCreateClasses(long currentUserId, long projectId, List<CreateClassRequestDto> classes)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId,  _cacheBusiness);
        
        // Bulk insert into classes; if there is a name collision, update the description and uuid if present
        var sql = @"
            INSERT INTO deeplynx.classes (project_id, name, description, uuid, last_updated_at, is_archived, last_updated_by)
            VALUES {0}
            ON CONFLICT (project_id, name) DO UPDATE SET
                description = COALESCE(EXCLUDED.description, classes.description),
                uuid = COALESCE(EXCLUDED.uuid, classes.uuid),
                last_updated_at = @now,
                last_updated_by = @lastUpdatedBy
           RETURNING id, project_id, name, description, uuid, last_updated_at, last_updated_by, is_archived;
        ";

        // establish "constant" parameters
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@projectId", projectId),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)),
            new NpgsqlParameter("@lastUpdatedBy", currentUserId)
        };
        
        // establish "dynamic" parameters (new for each dto in the list)
        parameters.AddRange(classes.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@p{i}_name", dto.Name),
            new NpgsqlParameter($"@p{i}_desc", (object?)dto.Description ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_uuid", (object?)dto.Uuid ?? DBNull.Value)
        }));
        
        // stringify the params and comma separate them
        var valueTuples = string.Join(", ", classes.Select((dto, i) =>
            $"(@projectId, @p{i}_name, @p{i}_desc, @p{i}_uuid, @now, false, @lastUpdatedBy)"));
        
        // put everything together and execute the query
        sql = string.Format(sql, valueTuples);
        
        // returns the resulting upserted classes
        var result = await _context.Database
            .SqlQueryRaw<ClassResponseDto>(sql, parameters.ToArray())
            .ToListAsync();

        // for each created class Bulk log events
        var events = new List<CreateEventRequestDto> { };
        foreach (var item in result)
        {
            events.Add(new CreateEventRequestDto
            {
                ProjectId = projectId,
                Operation = "create",
                EntityType = "class",
                EntityId = item.Id,
                EntityName = item.Name,
                DataSourceId = null,
                Properties = JsonSerializer.Serialize(new {item.Name}),
            });
        }
        await _eventBusiness.BulkCreateEvents(projectId, events);
        
        return result;
    }

    /// <summary>
    /// Updates an existing class by its ID.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">The ID of the project to which the class belongs.</param>
    /// <param name="classId">The ID of the class to update.</param>
    /// <param name="dto">The class request data transfer object containing updated class details.</param>
    /// <returns>The updated class response DTO with its details</returns>
    /// <exception cref="KeyNotFoundException">Returned if class not found or if ids missing</exception>
    public async Task<ClassResponseDto> UpdateClass(long currentUserId, long projectId, long classId, UpdateClassRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var updatedClass = await _context.Classes.FindAsync(classId);
        if (updatedClass == null || updatedClass.ProjectId != projectId || updatedClass.IsArchived)
        {
            throw new KeyNotFoundException($"Class with id {classId} not found");
        }

        updatedClass.ProjectId = projectId;
        updatedClass.Name = dto.Name ?? updatedClass.Name;
        updatedClass.Description = dto.Description ?? updatedClass.Description;
        updatedClass.Uuid = dto.Uuid ?? updatedClass.Uuid;
        updatedClass.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        updatedClass.LastUpdatedBy = currentUserId;

        _context.Classes.Update(updatedClass);
        await _context.SaveChangesAsync();
        
        // log event with class update details
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "update",
            EntityType = "class",
            EntityId = updatedClass.Id,
            EntityName = updatedClass.Name,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new {updatedClass.Name}),
        });

        return new ClassResponseDto
        {
            Id = updatedClass.Id,
            Name = updatedClass.Name,
            Description = updatedClass.Description,
            Uuid = updatedClass.Uuid,
            ProjectId = updatedClass.ProjectId,
            LastUpdatedAt = updatedClass.LastUpdatedAt,
            LastUpdatedBy = updatedClass.LastUpdatedBy,
            IsArchived = updatedClass.IsArchived
        };
    }

    /// <summary>
    /// Delete a class by id.
    /// </summary>
    /// <param name="projectId">ID of the project to which the class belongs.</param>
    /// <param name="classId">ID of the class to delete.</param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if class is not found.</exception>
    public async Task<bool> DeleteClass(long projectId, long classId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with id {projectId} not found.");

        var classToDelete = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId && c.ProjectId == projectId);
        if (classToDelete == null)
            throw new KeyNotFoundException($"Class with id {classId} not found");

        _context.Classes.Remove(classToDelete);
        await _context.SaveChangesAsync();
        
        return true;
    }

    /// <summary>
    /// Archive (soft delete) a class by id. This also archives downstream dependents.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">ID of the project to which the class belongs.</param>
    /// <param name="classId">ID of the class to archive.</param>
    /// <returns>Boolean true on successful archival.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if class is not found.</exception>
    /// <exception cref="DependencyDeletionException">Thrown if archival fails.</exception>
    public async Task<bool> ArchiveClass(long currentUserId, long projectId, long classId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var dbClass = await _context.Classes.FindAsync(classId);

        if (dbClass == null || dbClass.ProjectId != projectId || dbClass.IsArchived)
            throw new KeyNotFoundException($"Class with id {classId} not found");
            
        // set lastUpdatedAt timestamp
        var lastUpdatedAt = DateTime.UtcNow;
        
        //Todo: Add lastUpdatedBy to the sql procedure
        var lastUpdatedBy = currentUserId;

        // run archive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the archive class procedure, which archives this class
                // and all child objects with class_id as a foreign key
                var archived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.archive_class({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)",
                    classId, lastUpdatedAt
                );

                if (archived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException(
                        $"unable to archive class {classId} or its downstream dependents.");
                }
                
                await transaction.CommitAsync();
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException(
                    $"unable to archive class {classId} or its downstream dependents: {exc}");
            }
        }

        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "archive",
            EntityType = "class",
            EntityId = dbClass.Id,
            EntityName = dbClass.Name,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new { dbClass.Name }),
        });
        
        return true;
    }
        
        
    /// <summary>
    /// Unarchive a class by id. This also unarchives downstream dependents.
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">ID of the project to which the class belongs.</param>
    /// <param name="classId">ID of the class to unarchive.</param>
    /// <returns>Boolean true on successful unarchive.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if class is not found.</exception>
    /// <exception cref="DependencyDeletionException">Thrown if unarchive action fails.</exception>
    public async Task<bool> UnarchiveClass(long currentUserId, long projectId, long classId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var dbClass = await _context.Classes.FindAsync(classId);

        if (dbClass == null || dbClass.ProjectId != projectId || !dbClass.IsArchived)
            throw new KeyNotFoundException($"Class with id {classId} not found");
            
        // set lastUpdatedAt timestamp
        var lastUpdatedAt = DateTime.UtcNow;
        
        //Todo: Add lastUpdatedBy to the sql procedure
        var lastUpdatedBy = currentUserId;

        // run unarchive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the unarchive class procedure, which unarchives this class
                // and all child objects with class_id as a foreign key
                var unarchived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.unarchive_class({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)",
                    classId, lastUpdatedAt
                );

                if (unarchived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException(
                        $"unable to unarchive class {classId} or its downstream dependents.");
                }
                
                await transaction.CommitAsync();
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException(
                    $"unable to unarchive class {classId} or its downstream dependents: {exc}");
            }
        }

        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "unarchive",
            EntityType = "class",
            EntityId = dbClass.Id,
            EntityName = dbClass.Name,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new { dbClass.Name }),
        });
        
        return true;
    }

    /// <summary>
    /// This is used to get the general class that has been created with a project.
    /// If there is a project that does not have the class, it creates one. 
    /// </summary>
    /// <param name="currentUserId">ID of the User executing this method.</param>
    /// <param name="projectId">The ID of the project we are searching</param>
    /// <param name="className"> The name of the project class to search for</param>
    /// <returns>Class DTO of the found or created class</returns>
    public async Task<ClassResponseDto> GetClassInfo(long currentUserId, long projectId, string className)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var projectClass = await _context.Classes.FirstOrDefaultAsync(c => c.Name == className && c.ProjectId == projectId);

        if (projectClass != null)
        {
            return new ClassResponseDto()
            {
                Id = projectClass.Id,
                Name = projectClass.Name,
            };
        }

        var classDto = new CreateClassRequestDto()
        {
            Name = className
        };

        return await CreateClass(currentUserId, projectId, classDto);
    } 
    /// <summary>
    /// Validates that all provided class names exist in the database for the specified project.
    /// Used by MetadataBusiness to enforce ontologyMutable settings.
    /// </summary>
    /// <param name="projectId">The project ID to search within</param>
    /// <param name="classNames">List of class names to validate</param>
    /// <returns>List of classes that were found</returns>
    /// <exception cref="KeyNotFoundException">Thrown if one or more class names not found</exception>
    /// <exception cref="ArgumentException">Thrown if classNames list is null or empty</exception>
    public async Task<List<ClassResponseDto>> GetClassesByName(long projectId, List<string> classNames)
    {
        if (classNames == null || !classNames.Any())
            throw new ArgumentException("Class names list cannot be null or empty", nameof(classNames));

        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId,  _cacheBusiness);
    
        var cleanClassNames = classNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList();

        if (!cleanClassNames.Any())
            throw new ArgumentException("No valid class names provided", nameof(classNames));

        // Query for existing classes (excluding archived)
        var existingClasses = await _context.Classes
            .Where(c => c.ProjectId == projectId 
                        && !c.IsArchived
                        && cleanClassNames.Contains(c.Name))
            .ToListAsync();
    
        var foundClassNames = existingClasses.Select(c => c.Name).ToHashSet();
        var missingClassNames = cleanClassNames.Where(name => !foundClassNames.Contains(name)).ToList();

        if (missingClassNames.Any())
        {
            throw new KeyNotFoundException(
                $"Classes not found with names: {string.Join(", ", missingClassNames)}");
        }
    
        return existingClasses.Select(c => new ClassResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Uuid = c.Uuid,
            ProjectId = c.ProjectId,
            LastUpdatedAt = c.LastUpdatedAt,
            LastUpdatedBy = c.LastUpdatedBy,
            IsArchived = c.IsArchived
        }).ToList();
    }
}