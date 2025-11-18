using deeplynx.models;

namespace deeplynx.interfaces;

public interface IObjectStorageBusiness
{
    Task<List<ObjectStorageResponseDto>> GetAllObjectStorages(long? orgId, long? projectId, bool hideArchived);
    Task<ObjectStorageResponseDto> GetObjectStorage(long? orgId, long? projectId, long objectStorageId,  bool hideArchived);
    Task<ObjectStorageResponseDto> GetDefaultObjectStorage(long? orgId, long? projectId);
    Task<ObjectStorageResponseDto> CreateObjectStorage(long currentUserId, long? orgId, long? projectId, CreateObjectStorageRequestDto dto, bool makeDefault = false);
    Task<ObjectStorageResponseDto> UpdateObjectStorage(long currentUserId, long? orgId, long? projectId, long objectStorageId, UpdateObjectStorageRequestDto dto);
    Task<bool>  DeleteObjectStorage(long? orgId, long? projectId, long objectStorageId);
    Task<bool> ArchiveObjectStorage(long currentUserId, long? orgId, long? projectId, long objectStorageId);
    Task<bool> UnarchiveObjectStorage(long currentUserId, long? orgId, long? projectId, long objectStorageId);
    Task<ObjectStorageResponseDto> SetDefaultObjectStorage(long currentUserId, long? orgId, long? projectId, long objectStorageId);

}