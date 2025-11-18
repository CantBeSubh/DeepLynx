using deeplynx.models;

namespace deeplynx.interfaces;

public interface IClassBusiness
{
    Task<List<ClassResponseDto>> GetAllClasses(long projectId, bool hideArchived);
    Task<ClassResponseDto> GetClass(long projectId, long classId, bool hideArchived);
    Task<ClassResponseDto> CreateClass(long currentUserId, long projectId, CreateClassRequestDto dto);
    Task<List<ClassResponseDto>> BulkCreateClasses(long currentUserId, long projectId, List<CreateClassRequestDto> classRequestDtos);
    Task<ClassResponseDto> UpdateClass(long currentUserId, long projectId, long classId, UpdateClassRequestDto dto);
    Task<ClassResponseDto> GetClassInfo(long currentUserId, long projectId, string className);
    Task<bool> DeleteClass(long projectId, long classId);
    Task<bool> ArchiveClass(long currentUserId, long projectId, long classId);
    Task<bool> UnarchiveClass(long currentUserId, long projectId, long classId);
    Task<List<ClassResponseDto>> GetClassesByName(long projectId, List<string> classNames);
}