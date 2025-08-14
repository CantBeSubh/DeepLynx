using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class ObjectStorageBusiness: IObjectStorageBusiness
{
    private List<string> objectStorageTypes = ["filesystem", "azure_object", "aws_s3"];
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
            objectStoragesQuery = objectStoragesQuery.Where(c => c.ArchivedAt == null);
        }

        var objectStorages = await objectStoragesQuery.ToListAsync();
        return objectStorages
            .Select(os => new ObjectStorageResponseDto()
            {
                Id = os.Id,
                Name = os.Name,
                Type = os.Type,
                Default = os.Default,
                CreatedBy = os.CreatedBy,
                CreatedAt = os.CreatedAt,
                ModifiedBy = os.ModifiedBy,
                ModifiedAt = os.ModifiedAt,
                ArchivedAt = os.ArchivedAt,
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
        var objectStorage = await _context.ObjectStorages.Where(os => os.ProjectId == projectId && os.Id != objectStorageId).FirstOrDefaultAsync();
        
        if (objectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (hideArchived && objectStorage.ArchivedAt != null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} is archived");
        }

        return new ObjectStorageResponseDto
        {
            Id = objectStorage.Id,
            Name = objectStorage.Name,
            Type = objectStorage.Type,
            Default = objectStorage.Default,
            CreatedBy = objectStorage.CreatedBy,
            CreatedAt = objectStorage.CreatedAt,
            ModifiedBy = objectStorage.ModifiedBy,
            ModifiedAt = objectStorage.ModifiedAt,
            ArchivedAt = objectStorage.ArchivedAt,
        };
    }

    /// <summary>
    /// Creates an object storage
    /// </summary>
    /// <param name="projectId">The ID of the project to which the object storage belongs</param>
    /// <param name="dto">A data transfer object with details on the new object storage to be created.</param>
    /// <param name = "makeDefault"> A boolean that tells whether to make the object storage default or not</param>
    public async Task<ObjectStorageResponseDto> CreateObjectStorage(long projectId, CreateObjectStorageRequestDto dto, bool makeDefault = false)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        ValidationHelper.ValidateModel(dto);
        if (!objectStorageTypes.Contains(dto.Type))
        {
            throw new Exception($"Object storage type {dto.Type} not supported");
        }
        
        var config = new JsonObject();
        if (dto.Type == "azure_object")
        {
            config["connectionString"] = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
            config["containerName"] = Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER_NAME");
        }
        
        var newObjectStorage = new ObjectStorage
        {
            Name = dto.Name,
            Type = dto.Type,
            Default = makeDefault,
            ProjectId = projectId,
            Config = config.ToString(),
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null // TODO: Implement user ID here when JWT tokens are ready
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
            Default = newObjectStorage.Default,
            CreatedBy = newObjectStorage.CreatedBy,
            CreatedAt = newObjectStorage.CreatedAt,
            ModifiedBy = newObjectStorage.ModifiedBy,
            ModifiedAt = newObjectStorage.ModifiedAt,
            ArchivedAt = newObjectStorage.ArchivedAt,
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
        var updatedObjectStorage = await _context.ObjectStorages.Where(os => os.ProjectId == projectId && os.Id == objectStorageId).FirstOrDefaultAsync();
        if (updatedObjectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (updatedObjectStorage.ArchivedAt is not null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} is archived");
        }
        
        updatedObjectStorage.Name = dto.Name;
        updatedObjectStorage.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        updatedObjectStorage.ModifiedBy = null; // TODO: Implement user ID here when JWT tokens are ready
        
        _context.ObjectStorages.Update(updatedObjectStorage);
        await _context.SaveChangesAsync();

        return new ObjectStorageResponseDto
        {
            Id = updatedObjectStorage.Id,
            Name = updatedObjectStorage.Name,
            Type = updatedObjectStorage.Type,
            Default = updatedObjectStorage.Default,
            CreatedAt = updatedObjectStorage.CreatedAt,
            CreatedBy = updatedObjectStorage.CreatedBy,
            ModifiedBy = updatedObjectStorage.ModifiedBy,
            ModifiedAt = updatedObjectStorage.ModifiedAt,
            ArchivedAt = updatedObjectStorage.ArchivedAt,
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
        var objectStorage = await _context.ObjectStorages.Where(os => os.ProjectId == projectId && os.Id == objectStorageId).FirstOrDefaultAsync();
        
        if (objectStorage == null)
        { 
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
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
        var objectStorage = await _context.ObjectStorages.Where(os => os.ProjectId == projectId && os.Id == objectStorageId).FirstOrDefaultAsync();
        if (objectStorage == null || objectStorage.ArchivedAt is not null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (objectStorage.ArchivedAt is not null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} is already archived");
        }
        
        objectStorage.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
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
        var objectStorage = await _context.ObjectStorages.Where(os => os.ProjectId == projectId && os.Id == objectStorageId).FirstOrDefaultAsync();
        if (objectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (objectStorage.ArchivedAt is null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} is not archived already");
        }
        
        objectStorage.ArchivedAt = null;
        _context.ObjectStorages.Update(objectStorage);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeDefaultStorageObject(long projectId, long objectStorageId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var defaultObjectStorage = await _context.ObjectStorages.Where(os => os.ProjectId == projectId && os.Id == objectStorageId).FirstOrDefaultAsync();
        if (defaultObjectStorage == null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} not found");
        }

        if (defaultObjectStorage.ArchivedAt is null)
        {
            throw new KeyNotFoundException($"Object storage with id {objectStorageId} is archived");
        }

        if (defaultObjectStorage.Default)
        {
            throw new Exception($"Object storage with id {objectStorageId} is already default");
        }
        
        defaultObjectStorage.Default = true;
        await MakePreviousDefaultsFalse(projectId, defaultObjectStorage.Id);
        _context.ObjectStorages.Update(defaultObjectStorage);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task MakePreviousDefaultsFalse(long projectId, long defaultObjectStorageId)
    {
        var previousDefaults = await _context.ObjectStorages.Where(os => os.ProjectId == projectId && os.Default == true && os.Id != defaultObjectStorageId).ToListAsync();
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