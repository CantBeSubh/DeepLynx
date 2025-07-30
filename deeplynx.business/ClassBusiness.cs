using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using deeplynx.helpers;
using Npgsql;

namespace deeplynx.business;

public class ClassBusiness : IClassBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IEdgeMappingBusiness _edgeMappingBusiness;
    private readonly IRecordBusiness _recordBusiness;
    private readonly IRecordMappingBusiness _recordMappingBusiness;
    private readonly IRelationshipBusiness _relationshipBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for class operations</param>
    /// <param name="edgeMappingBusiness">Passed in context of edge mapping objects</param>
    /// <param name="recordBusiness">Passed in context of record objects</param>
    /// <param name="recordMappingBusiness">Passed in context of record mapping objects</param>
    /// <param name="relationshipBusiness">Passed in context of relationship objects</param>
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

    /// <summary>
    /// Retrieves all classes
    /// </summary>
    /// <param name="projectId">The ID of the project to which the class belongs</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived classes from the result</param>
    /// <returns>A list of classes</returns>
    public async Task<List<ClassResponseDto>> GetAllClasses(long projectId, bool hideArchived)
    {
        DoesProjectExist(projectId, hideArchived);
        
        var classes = await _context.Classes
            .Where(c => c.ProjectId == projectId).ToListAsync();
        
        if (hideArchived)
        {
            classes = classes.Where(c => c.ArchivedAt == null).ToList();
        }
        
        return classes 
            .Select(c => new ClassResponseDto()
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Uuid = c.Uuid,
                ProjectId = c.ProjectId,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                ModifiedBy = c.ModifiedBy,
                ModifiedAt = c.ModifiedAt,
                ArchivedAt = c.ArchivedAt,
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
        DoesProjectExist(projectId, hideArchived);
        var newClass = await _context.Classes
            .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Id == classId);
        if (newClass == null)
        {
            throw new KeyNotFoundException($"Class with id {classId} not found");
        }
        
        if (hideArchived && newClass.ArchivedAt != null)
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
            CreatedBy = newClass.CreatedBy,
            CreatedAt = newClass.CreatedAt,
            ModifiedBy = newClass.ModifiedBy,
            ModifiedAt = newClass.ModifiedAt,
            ArchivedAt = newClass.ArchivedAt
        };
    }

    /// <summary>
    /// Creates a new class based on the data transfer object supplied.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the class belongs</param>
    /// <param name="dto">A data transfer object with details on the new class to be created.</param>
    /// <returns>The new class which was just created.</returns>
    /// <exception cref="KeyNotFoundException">Returned if class not found</exception>
    /// <exception cref="Exception">Returned if class already exists</exception>
    public async Task<ClassResponseDto> CreateClass(long projectId, ClassRequestDto dto)
    {
        DoesProjectExist(projectId);
        ValidationHelper.ValidateModel(dto);

        var newClass = new Class
        {
            ProjectId = projectId,
            Name = dto.Name,
            Description = dto.Description,
            Uuid = dto.Uuid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null, // TODO: Implement user ID here when JWT tokens are ready
            ModifiedBy = null  // TODO: Implement user ID here when JWT tokens are ready

        };

        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();

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
    
    /// <summary>
    /// Creates a new classes based on the data transfer object supplied.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the class belongs</param>
    /// <param name="classes">A list of class data transfer object with details on the new class to be created.</param>
    /// <returns>The new class which was just created.</returns>
    /// <exception cref="Exception">Returned if class already exists</exception>
    public async Task<List<ClassResponseDto>> BulkCreateClasses(long projectId, List<ClassRequestDto> classes)
    {
        DoesProjectExist(projectId);
        
        // Bulk insert into classes; if there is a name collision, update the description and uuid if present
        var sql = @"
            INSERT INTO deeplynx.classes (project_id, name, description, uuid, created_at)
            VALUES {0}
            ON CONFLICT (project_id, name) DO UPDATE SET
                description = COALESCE(EXCLUDED.description, classes.description),
                uuid = COALESCE(EXCLUDED.uuid, classes.uuid),
                modified_at = @now
            RETURNING *;
        ";

        // establish "constant" parameters
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@projectId", projectId),
            new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
        };
        
        // establish "dynamic" parameters (new for each dto in the list)
        parameters.AddRange(classes.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@p{i}_name", dto.Name),
            new NpgsqlParameter($"@p{i}_desc", (object?)dto.Description ?? DBNull.Value),
            new NpgsqlParameter($"@p{i}_uuid", (object?)dto.Uuid ?? DBNull.Value),
        }));
        
        // stringify the params and comma separate them
        var valueTuples = string.Join(", ", classes.Select((dto, i) =>
            $"(@projectId, @p{i}_name, @p{i}_desc, @p{i}_uuid, @now)"));
        
        // put everything together and execute the query
        sql = string.Format(sql, valueTuples);

        // returns the resulting upserted classes
        return await _context.Database
            .SqlQueryRaw<ClassResponseDto>(sql, parameters.ToArray())
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing class by its ID.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the class belongs.</param>
    /// <param name="classId">The ID of the class to update.</param>
    /// <param name="dto">The class request data transfer object containing updated class details.</param>
    /// <returns>The updated class response DTO with its details</returns>
    /// <exception cref="KeyNotFoundException">Returned if class not found or if ids missing</exception>
    public async Task<ClassResponseDto> UpdateClass(long projectId, long classId, ClassRequestDto dto)
    {
        DoesProjectExist(projectId);
        var updatedClass = await _context.Classes.FindAsync(classId);
        if (updatedClass == null || updatedClass.ProjectId != projectId || updatedClass.ArchivedAt is not null)
        {
            throw new KeyNotFoundException($"Class with id {classId} not found");
        }

        updatedClass.ProjectId = projectId;
        updatedClass.Name = dto.Name;
        updatedClass.Description = dto.Description;
        updatedClass.Uuid = dto.Uuid;
        updatedClass.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        updatedClass.ModifiedBy = null;  // TODO: Implement user ID here when JWT tokens are ready

        _context.Classes.Update(updatedClass);
        await _context.SaveChangesAsync();

        return new ClassResponseDto
        {
            Id = updatedClass.Id,
            Name = updatedClass.Name,
            Description = updatedClass.Description,
            Uuid = updatedClass.Uuid,
            ProjectId = updatedClass.ProjectId,
            ModifiedBy = updatedClass.ModifiedBy,
            ModifiedAt = updatedClass.ModifiedAt,
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
    /// <param name="projectId">ID of the project to which the class belongs.</param>
    /// <param name="classId">ID of the class to archive.</param>
    /// <returns>Boolean true on successful archival.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if class is not found.</exception>
    /// <exception cref="DependencyDeletionException">Thrown if archival fails.</exception>
    public async Task<bool> ArchiveClass(long projectId, long classId)
    {
        DoesProjectExist(projectId);
        // using dbClass since "class" is a reserved word
        var dbClass = await _context.Classes.FindAsync(classId);

        if (dbClass == null || dbClass.ProjectId != projectId || dbClass.ArchivedAt is not null)
            throw new KeyNotFoundException("Class not found.");

        // set archivedAt timestamp
        var archivedAt = DateTime.UtcNow;

        // run archive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the archive class procedure, which archives this class
                // and all child objects with class_id as a foreign key
                var archived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.archive_class({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)", classId, archivedAt);

                if (archived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException($"unable to archive class {classId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException($"unable to archive class {classId} or its downstream dependents: {exc}");
            }
        }
    }
    
    /// <summary>
    /// Unarchive a class by id. This also unarchives downstream dependents.
    /// </summary>
    /// <param name="projectId">ID of the project to which the class belongs.</param>
    /// <param name="classId">ID of the class to unarchive.</param>
    /// <returns>Boolean true on successful unarchive.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if class is not found.</exception>
    /// <exception cref="DependencyDeletionException">Thrown if unarchive action fails.</exception>
    public async Task<bool> UnarchiveClass(long projectId, long classId)
    {
        DoesProjectExist(projectId);
        // using dbClass since "class" is a reserved word
        var dbClass = await _context.Classes.FindAsync(classId);

        if (dbClass == null || dbClass.ProjectId != projectId || dbClass.ArchivedAt is null)
            throw new KeyNotFoundException("Class not found or is not archived.");

        // run unarchive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the unarchive class procedure, which unarchives this class
                // and all child objects with class_id as a foreign key
                var unarchived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.unarchive_class({0}::INTEGER)", classId);

                if (unarchived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException($"unable to unarchive class {classId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException($"unable to unarchive class {classId} or its downstream dependents: {exc}");
            }
        }
    }

    /// <summary>
    /// This is used to get the general class that has been created with a project.
    /// If there is a project that does not have the class, it creates one. 
    /// </summary>
    /// <param name="projectId">The ID of the project we are searching</param>
    /// <param name="className"> The name of the project class to search for</param>
    /// <returns>Class DTO of the found or created class</returns>
    public async Task<ClassResponseDto> GetClassInfo(long projectId, string className)
    {
        DoesProjectExist(projectId);
        var projectClass = await _context.Classes.FirstOrDefaultAsync(c => c.Name == className && c.ProjectId == projectId);

        if (projectClass != null)
        {
            return new ClassResponseDto()
            {
                Id = projectClass.Id,
                Name = projectClass.Name,
            };
        }

        var classDto = new ClassRequestDto()
        {
            Name = className
        };

        return await CreateClass(projectId, classDto);
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