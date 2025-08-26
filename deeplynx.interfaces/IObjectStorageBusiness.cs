using deeplynx.models;

namespace deeplynx.interfaces;

public interface IObjectStorageBusiness
{
    Task<List<ObjectStorageResponseDto>> GetAllObjectStorages(long projectId, bool hideArchived);
    Task<ObjectStorageResponseDto> GetObjectStorage(long projectId, long objectStorageId,  bool hideArchived);
    Task<ObjectStorageResponseDto> GetDefaultObjectStorage(long projectId);
    Task<ObjectStorageResponseDto> CreateObjectStorage(long projectId, CreateObjectStorageRequestDto dto, bool makeDefault = false);
    Task<ObjectStorageResponseDto> UpdateObjectStorage(long projectId, long objectStorageId, UpdateObjectStorageRequestDto dto);
    Task<bool>  DeleteObjectStorage(long projectId, long objectStorageId);
    Task<bool> ArchiveObjectStorage(long projectId, long objectStorageId);
    Task<bool> UnarchiveObjectStorage(long projectId, long objectStorageId);
    Task<ObjectStorageResponseDto> SetDefaultObjectStorage(long projectId, long objectStorageId);

}