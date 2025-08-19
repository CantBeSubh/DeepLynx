using deeplynx.models;

namespace deeplynx.interfaces;

public interface IObjectStorageBusiness
{
    // TODO: get all object storage configs
    // TODO: get one object storage config
    // TODO: create object storage config
    // TODO: update object storage config
    // TODO: delete object storage config
    // TODO: archive object storage config
    // TODO: unarchive object storage config
    Task<List<ObjectStorageResponseDto>> GetAllObjectStorages(long projectId, bool hideArchived);
    Task<ObjectStorageResponseDto> GetObjectStorage(long projectId, long objectStorageId,  bool hideArchived);
    Task<ObjectStorageResponseDto> CreateObjectStorage(long projectId, CreateObjectStorageRequestDto dto, bool makeDefault = false);
    Task<ObjectStorageResponseDto> UpdateObjectStorage(long projectId, long objectStorageId, UpdateObjectStorageRequestDto dto);
    Task<bool>  DeleteObjectStorage(long projectId, long objectStorageId);
    Task<bool> ArchiveObjectStorage(long projectId, long objectStorageId);
    Task<bool> UnarchiveObjectStorage(long projectId, long objectStorageId);
    Task<ObjectStorageResponseDto> ChangeDefaultObjectStorage(long projectId, long objectStorageId);

}