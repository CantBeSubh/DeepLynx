using System.Data;
using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class ObjectStorageBusiness: IObjectStorageBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ICacheBusiness _cacheBusiness;
    public ObjectStorageBusiness(DeeplynxContext context, ICacheBusiness cacheBusiness)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
    }
    
    /// <summary>
    /// Gets all the object storages for a project
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storages belong</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived ObjectStorages from the result </param>
    public async Task<List<ObjectStorageResponseDto>> GetAllObjectStorages(long? organizationId, long? projectId, bool hideArchived)
    {
        if (!projectId.HasValue && !organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified");
        }
        if (projectId.HasValue && organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified, not both");
        }

        IQueryable<ObjectStorage> query = _context.ObjectStorages;

        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
            query = query.Where(os => os.ProjectId == projectId);
        }
        else // organizationId.HasValue is guaranteed by the first check
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
            query = query.Where(os => os.OrganizationId == organizationId);
        }

        if (hideArchived)
        {
            query = query.Where(os => !os.IsArchived);
        }

        var objectStorages = await query.ToListAsync();
        return objectStorages
            .Select(os => new ObjectStorageResponseDto()
            {
                Id = os.Id,
                Name = os.Name,
                Type = os.Type,
                ProjectId = os.ProjectId,
                OrganizationId = os.OrganizationId,
                Default = os.Default,
                LastUpdatedAt = os.LastUpdatedAt,
                LastUpdatedBy = os.LastUpdatedBy,
                IsArchived = os.IsArchived,
            }).ToList();

    }

    /// <summary>
    /// Gets a single object storage
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <param name="objectStorageId">ID of object storage</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived object storage from the result</param>
    /// <exception cref="KeyNotFoundException">Thrown when the object storage is not found or archived</exception>
    public async Task<ObjectStorageResponseDto> GetObjectStorage(long? organizationId, long? projectId, long objectStorageId, 
        bool hideArchived)
    {
        if (!projectId.HasValue && !organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified");
        }

        if (projectId.HasValue && organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified, not both");
        }
    
        var query = _context.ObjectStorages
            .Where(os => os.Id == objectStorageId);
    
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
            query = query.Where(os => os.ProjectId == projectId);
        }
        else // organizationId.HasValue is guaranteed by the first check
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
            query = query.Where(os => os.OrganizationId == organizationId);
        }
        
        if (hideArchived)
        {
            query = query.Where(os => !os.IsArchived);
        }
    
        var objectStorage = await query
            .Select(os => new ObjectStorageResponseDto
            {
                Id = os.Id,
                Name = os.Name,
                Type = os.Type,
                ProjectId = os.ProjectId,
                OrganizationId = os.OrganizationId,
                Default = os.Default,
                LastUpdatedAt = os.LastUpdatedAt,
                LastUpdatedBy = os.LastUpdatedBy,
                IsArchived = os.IsArchived,
            })
            .FirstOrDefaultAsync();
    
        if (objectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} in {(projectId.HasValue ? $"project {projectId.Value}" : $"organization {organizationId.Value}")} not found");
        }
        
        return objectStorage;
    }
    
    /// <summary>
    /// Gets default object storage for project
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <exception cref="KeyNotFoundException">Thrown when the object storage is not found or archived</exception>
    public async Task<ObjectStorageResponseDto> GetDefaultObjectStorage(long? organizationId, long? projectId)
    {
        if (!projectId.HasValue && !organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified");
        }
        
        if (projectId.HasValue && organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified, not both");
        }

        var query = _context.ObjectStorages.Where(os => os.Default && !os.IsArchived);
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
            query = query.Where(os => os.ProjectId == projectId);
        }
        else
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
            query = query.Where(os => os.OrganizationId == organizationId);
        }
    
        var objectStorage = await query
            .Select(os => new ObjectStorageResponseDto
            {
                Id = os.Id,
                Name = os.Name,
                Type = os.Type,
                ProjectId = os.ProjectId,
                OrganizationId = os.OrganizationId,
                Default = os.Default,
                LastUpdatedAt = os.LastUpdatedAt,
                LastUpdatedBy = os.LastUpdatedBy,
                IsArchived = os.IsArchived,
            })
            .FirstOrDefaultAsync();
    
        if (objectStorage == null)
        {
            throw new KeyNotFoundException($"Default object storage for {(projectId.HasValue ? $"project {projectId.Value}" : $"organization {organizationId.Value}")} not found");
        }
        
        return objectStorage;
    }

    /// <summary>
    /// Creates an object storage
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <param name="dto">A data transfer object with details on the new object storage to be created.</param>
    /// <param name = "makeDefault"> A boolean that tells whether to make the object storage default or not</param>
    public async Task<ObjectStorageResponseDto> CreateObjectStorage(
        long? organizationId, 
        long? projectId,
        CreateObjectStorageRequestDto dto,
        bool makeDefault = false)
    {
        if (!projectId.HasValue && !organizationId.HasValue)
        {
            throw new ArgumentException("Must specify project or organization ID");
        }

        if (projectId.HasValue && organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified, not both");
        }
        
        ValidationHelper.ValidateModel(dto);


        bool objectStorageWithSameName;
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
            objectStorageWithSameName = await _context.ObjectStorages.AnyAsync(os => os.ProjectId == projectId && os.Name == dto.Name);
        }
        else
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
            objectStorageWithSameName = await _context.ObjectStorages.AnyAsync(os => os.OrganizationId == organizationId && os.Name == dto.Name);
        }
        
        if (objectStorageWithSameName)
        {
            throw new InvalidOperationException($"Object storage with name {dto.Name} in {(projectId.HasValue ? $"project {projectId.Value}" : $"organization {organizationId.Value}")} already exists");
        }
        
        string type;
        if (dto.Config.ContainsKey("mountPath"))
        {
            type = "filesystem";
        } else if (dto.Config.ContainsKey("azureConnectionString"))
        {
            type = "azure_object";
        } else if (dto.Config.ContainsKey("awsConnectionString"))
        {
            type = "aws_s3";
        }
        else
        {
            throw new KeyNotFoundException("Request does not contain recognized config");
        }
        
        var newObjectStorage = new ObjectStorage
        {
            Name = dto.Name,
            Type = type,
            Default = makeDefault,
            ProjectId = projectId,
            OrganizationId = organizationId,
            Config = dto.Config.ToString(),
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null // TODO: Implement user ID here when JWT tokens are ready
        };
        
        _context.ObjectStorages.Add(newObjectStorage);
        
        if (makeDefault && projectId.HasValue)
            await MakePreviousDefaultsFalse(organizationId, projectId, newObjectStorage.Id);
        
        await _context.SaveChangesAsync();

        return new ObjectStorageResponseDto
        {
            Id = newObjectStorage.Id,
            Name = newObjectStorage.Name,
            Type = newObjectStorage.Type,
            ProjectId = newObjectStorage.ProjectId,
            OrganizationId = newObjectStorage.OrganizationId,
            Default = newObjectStorage.Default,
            LastUpdatedAt = newObjectStorage.LastUpdatedAt,
            LastUpdatedBy = newObjectStorage.LastUpdatedBy,
            IsArchived = newObjectStorage.IsArchived,
        };
    }
    
    /// <summary>
    /// Updates an object storage
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <param name="objectStorageId">ID of object storage</param>
    /// <param name="dto">A data transfer object with details on object storage fields to be updated</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<ObjectStorageResponseDto> UpdateObjectStorage(long? organizationId, long? projectId, long objectStorageId,
        UpdateObjectStorageRequestDto dto)
    {
        if (!projectId.HasValue && !organizationId.HasValue)
        {
            throw new ArgumentException("Must specify project or organization ID");
        }

        if (projectId.HasValue && organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified, not both");
        }
        
        ValidationHelper.ValidateModel(dto);

        var query = _context.ObjectStorages.Where(os => os.Id == objectStorageId && !os.IsArchived);
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
            query = query.Where(os => os.ProjectId == projectId);
        }
        else
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
            query = query.Where(os => os.OrganizationId == organizationId);
        }

        var updatedObjectStorage = await query.FirstOrDefaultAsync();
        
        if (updatedObjectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }
        
        updatedObjectStorage.Name = dto.Name;
        updatedObjectStorage.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        updatedObjectStorage.LastUpdatedBy = null; // TODO: Implement user ID here when JWT tokens are ready
        
        await _context.SaveChangesAsync();

        return new ObjectStorageResponseDto
        {
            Id = updatedObjectStorage.Id,
            Name = updatedObjectStorage.Name,
            Type = updatedObjectStorage.Type,
            ProjectId = updatedObjectStorage.ProjectId,
            Default = updatedObjectStorage.Default,
            LastUpdatedAt = updatedObjectStorage.LastUpdatedAt,
            LastUpdatedBy = updatedObjectStorage.LastUpdatedBy,
            IsArchived = updatedObjectStorage.IsArchived,
        };

    }
    
    /// <summary>
    /// Hard deletes an object storage
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <param name="objectStorageId">ID of object storage</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<bool> DeleteObjectStorage(long? organizationId, long? projectId, long objectStorageId)
    {
        if (!projectId.HasValue && !organizationId.HasValue)
        {
            throw new ArgumentException("Must specify project or organization ID");
        }

        if (projectId.HasValue && organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified, not both");
        }
        
        var query = _context.ObjectStorages.Where(os => os.Id == objectStorageId);
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
            query = query.Where(os => os.ProjectId == projectId);
        }
        else
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
            query = query.Where(os => os.OrganizationId == organizationId);
        }
        
        
        var objectStorage = await query.FirstOrDefaultAsync();
        
        if (objectStorage == null)
        { 
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (objectStorage.Default)
        {
            throw new InvalidOperationException("Default object storage cannot be deleted." +
                                                " Please assign new default storage before deleting.");
        }
        _context.ObjectStorages.Remove(objectStorage);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Soft deletes (archives) a data storage
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <param name="objectStorageId">ID of object storage</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<bool> ArchiveObjectStorage(long? organizationId, long? projectId, long objectStorageId)
    {
        if (!projectId.HasValue && !organizationId.HasValue)
        {
            throw new ArgumentException("Must specify project or organization ID");
        }

        if (projectId.HasValue && organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified, not both");
        }
        
        var query = _context.ObjectStorages.Where(os => os.Id == objectStorageId);
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
            query = query.Where(os => os.ProjectId == projectId);
        }
        else
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
            query = query.Where(os => os.OrganizationId == organizationId);
        }
        
        var objectStorage = await query.FirstOrDefaultAsync();
        
        if (objectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (objectStorage.IsArchived)
        {
            throw new InvalidOperationException($"Object storage with id {objectStorageId} is already archived");
        }

        if (objectStorage.Default)
        {
            throw new InvalidOperationException("Default object storage cannot be archived." +
                                                " Please assign new default storage before archiving.");
        }

        objectStorage.IsArchived = true;
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Unarchives a data storage
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <param name="objectStorageId">ID of object storage</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<bool> UnarchiveObjectStorage(long? organizationId, long? projectId, long objectStorageId)
    {
        if (!projectId.HasValue && !organizationId.HasValue)
        {
            throw new ArgumentException("Must specify project or organization ID");
        }

        if (projectId.HasValue && organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified, not both");
        }
        
        var query = _context.ObjectStorages.Where(os => os.Id == objectStorageId);
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
            query = query.Where(os => os.ProjectId == projectId);
        }
        else
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
            query = query.Where(os => os.OrganizationId == organizationId);
        }
        
        var objectStorage = await query.FirstOrDefaultAsync();
        
        if (objectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (!objectStorage.IsArchived)
        {
            throw new InvalidOperationException($"Object storage with id {objectStorageId} is not archived already");
        }
        
        objectStorage.IsArchived = false;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Changes the default object storage
    /// </summary>
    /// <param name="projectId">ID of the project in which the object storage belongs</param>
    /// <param name="objectStorageId">ID of the object storage to change to default</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<ObjectStorageResponseDto> SetDefaultObjectStorage(long? organizationId, long? projectId, long objectStorageId)
    {
        if (!projectId.HasValue && !organizationId.HasValue)
        {
            throw new ArgumentException("Must specify project or organization ID");
        }

        if (projectId.HasValue && organizationId.HasValue)
        {
            throw new ArgumentException("Either projectId or organizationId must be specified, not both");
        }
        
        var query = _context.ObjectStorages.Where(os => os.Id == objectStorageId);
        if (projectId.HasValue)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId.Value, _cacheBusiness);
            query = query.Where(os => os.ProjectId == projectId);
        }
        else
        {
            await ExistenceHelper.EnsureOrganizationExistsAsync(_context, organizationId.Value);
            query = query.Where(os => os.OrganizationId == organizationId);
        }
        
        var defaultObjectStorage = await query.FirstOrDefaultAsync();
        
        if (defaultObjectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (defaultObjectStorage.IsArchived)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} is archived");
        }

        if (!defaultObjectStorage.Default)
        {
            defaultObjectStorage.Default = true;
            defaultObjectStorage.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            defaultObjectStorage.LastUpdatedBy = null; // TODO: handled in future by JWT.

            await MakePreviousDefaultsFalse(organizationId, projectId, defaultObjectStorage.Id);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException($"Object storage with id {objectStorageId} is already default");
        }

        return new ObjectStorageResponseDto
        {
            Id = defaultObjectStorage.Id,
            Name = defaultObjectStorage.Name,
            Type = defaultObjectStorage.Type,
            ProjectId = defaultObjectStorage.ProjectId,
            Default = defaultObjectStorage.Default,
            LastUpdatedAt = defaultObjectStorage.LastUpdatedAt,
            LastUpdatedBy = defaultObjectStorage.LastUpdatedBy,
            IsArchived = defaultObjectStorage.IsArchived,
        };
    }

    private async Task MakePreviousDefaultsFalse(long? organizationId, long? projectId, long defaultObjectStorageId)
    {
        var defaultsQuery = _context.ObjectStorages.Where(os => os.Default && os.Id != defaultObjectStorageId);

        if (projectId.HasValue)
        {
            defaultsQuery = defaultsQuery.Where(os => os.ProjectId == projectId.Value);
        } 
        else if (organizationId.HasValue)
        {
            defaultsQuery = defaultsQuery.Where(os => os.OrganizationId == organizationId.Value);
        }
        
        var previousDefaults = await defaultsQuery.ToListAsync();
        
        if (previousDefaults.Count > 0)
        {
            foreach (var previousDefault in previousDefaults)
            {
                previousDefault.Default = false;
                _context.ObjectStorages.Update(previousDefault);
            }
        }
    }
    
}