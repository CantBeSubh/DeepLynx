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
    public ObjectStorageBusiness(DeeplynxContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets all the object storages for a project
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storages belong</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived ObjectStorages from the result </param>
    public async Task<List<ObjectStorageResponseDto>> GetAllObjectStorages(long projectId, bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        
        var objectStoragesQuery = _context.ObjectStorages.Where(os => os.ProjectId == projectId);

        if (hideArchived)
        {
            objectStoragesQuery = objectStoragesQuery.Where(os => !os.IsArchived);
        }

        var objectStorages = await objectStoragesQuery.ToListAsync();
        return objectStorages
            .Select(os => new ObjectStorageResponseDto()
            {
                Id = os.Id,
                Name = os.Name,
                Type = os.Type,
                ProjectId = os.ProjectId,
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
    public async Task<ObjectStorageResponseDto> GetObjectStorage(long projectId, long objectStorageId, 
        bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var objectStorage =
            await _context.ObjectStorages
                .Where(os => os.ProjectId == projectId && os.Id == objectStorageId)
                .FirstOrDefaultAsync();
        
        if (objectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (hideArchived && objectStorage.IsArchived)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} is archived");
        }

        return new ObjectStorageResponseDto
        {
            Id = objectStorage.Id,
            Name = objectStorage.Name,
            Type = objectStorage.Type,
            ProjectId = objectStorage.ProjectId,
            Default = objectStorage.Default,
            LastUpdatedAt = objectStorage.LastUpdatedAt,
            LastUpdatedBy = objectStorage.LastUpdatedBy,
            IsArchived = objectStorage.IsArchived,
        };
    }
    
    /// <summary>
    /// Gets default object storage for project
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <exception cref="KeyNotFoundException">Thrown when the object storage is not found or archived</exception>
    public async Task<ObjectStorageResponseDto> GetDefaultObjectStorage(long projectId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var objectStorage = await _context.ObjectStorages
                .Where(os => os.ProjectId == projectId && os.Default == true)
                .FirstOrDefaultAsync();
        
        if (objectStorage == null)
        {
            throw new KeyNotFoundException($"Default object storage for project {projectId} not found");
        }
        
        //Todo: update stored procedure to mark archived object storages default column as false
        if (objectStorage.IsArchived)
        {
            throw new KeyNotFoundException($"Found archived default object storage");
        }

        return new ObjectStorageResponseDto
        {
            Id = objectStorage.Id,
            Name = objectStorage.Name,
            Type = objectStorage.Type,
            ProjectId = objectStorage.ProjectId,
            Default = objectStorage.Default,
            LastUpdatedAt = objectStorage.LastUpdatedAt,
            LastUpdatedBy = objectStorage.LastUpdatedBy,
            IsArchived = objectStorage.IsArchived,
        };
    }

    /// <summary>
    /// Creates an object storage
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <param name="dto">A data transfer object with details on the new object storage to be created.</param>
    /// <param name = "makeDefault"> A boolean that tells whether to make the object storage default or not</param>
    public async Task<ObjectStorageResponseDto> CreateObjectStorage(
        long projectId,
        CreateObjectStorageRequestDto dto,
        bool makeDefault = false)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        ValidationHelper.ValidateModel(dto);

        var objectStorageWithSameName = 
            await _context.ObjectStorages.FirstOrDefaultAsync(os => os.ProjectId == projectId && os.Name == dto.Name);
        if (objectStorageWithSameName != null)
        {
            throw new InvalidOperationException($"Object storage with name {dto.Name} already exists");
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
            Config = dto.Config.ToString(),
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null // TODO: Implement user ID here when JWT tokens are ready
        };
        
        _context.ObjectStorages.Add(newObjectStorage);
        
        if (makeDefault)
            await MakePreviousDefaultsFalse(projectId, newObjectStorage.Id);
        
        await _context.SaveChangesAsync();

        return new ObjectStorageResponseDto
        {
            Id = newObjectStorage.Id,
            Name = newObjectStorage.Name,
            Type = newObjectStorage.Type,
            ProjectId = newObjectStorage.ProjectId,
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
    public async Task<ObjectStorageResponseDto> UpdateObjectStorage(long projectId, long objectStorageId,
        UpdateObjectStorageRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        ValidationHelper.ValidateModel(dto);
        var updatedObjectStorage = 
            await _context.ObjectStorages
            .Where(os => os.ProjectId == projectId && os.Id == objectStorageId)
            .FirstOrDefaultAsync();
        if (updatedObjectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (updatedObjectStorage.IsArchived)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} is archived");
        }
        
        updatedObjectStorage.Name = dto.Name;
        updatedObjectStorage.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        updatedObjectStorage.LastUpdatedBy = null; // TODO: Implement user ID here when JWT tokens are ready
        
        _context.ObjectStorages.Update(updatedObjectStorage);
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
    public async Task<bool> DeleteObjectStorage(long projectId, long objectStorageId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        
        var objectStorage = 
            await _context.ObjectStorages
                .Where(os => os.ProjectId == projectId && os.Id == objectStorageId)
                .FirstOrDefaultAsync();
        
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
    public async Task<bool> ArchiveObjectStorage(long projectId, long objectStorageId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        
        var objectStorage =
            await _context.ObjectStorages
                .Where(os => os.ProjectId == projectId && os.Id == objectStorageId)
                .FirstOrDefaultAsync();
        
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
        _context.ObjectStorages.Update(objectStorage);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Unarchives a data storage
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <param name="objectStorageId">ID of object storage</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<bool> UnarchiveObjectStorage(long projectId, long objectStorageId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        
        var objectStorage = 
            await _context.ObjectStorages
                .Where(os => os.ProjectId == projectId && os.Id == objectStorageId)
                .FirstOrDefaultAsync();
        
        if (objectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (!objectStorage.IsArchived)
        {
            throw new InvalidOperationException($"Object storage with id {objectStorageId} is not archived already");
        }
        
        objectStorage.IsArchived = false;
        _context.ObjectStorages.Update(objectStorage);
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
    public async Task<ObjectStorageResponseDto> SetDefaultObjectStorage(long projectId, long objectStorageId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        
        var defaultObjectStorage = 
            await _context.ObjectStorages
                .Where(os => os.ProjectId == projectId && os.Id == objectStorageId)
                .FirstOrDefaultAsync();
        
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
            defaultObjectStorage.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            defaultObjectStorage.ModifiedBy = null; // TODO: handled in future by JWT.

            await MakePreviousDefaultsFalse(projectId, defaultObjectStorage.Id);
            _context.ObjectStorages.Update(defaultObjectStorage);
            await _context.SaveChangesAsync();
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

    private async Task MakePreviousDefaultsFalse(long projectId, long defaultObjectStorageId)
    {
        var previousDefaults = 
            await _context.ObjectStorages
                .Where(os => os.ProjectId == projectId && os.Default == true && os.Id != defaultObjectStorageId)
                .ToListAsync();
        
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