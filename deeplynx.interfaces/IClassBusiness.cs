using deeplynx.models;

namespace deeplynx.interfaces;

public interface IClassBusiness
{
    Task<List<ClassResponseDto>> GetAllClasses(long[] projectIds, bool hideArchived);
    Task<ClassResponseDto> GetClass(long projectId, long classId, bool hideArchived);
    Task<ClassResponseDto> CreateClass(long projectId, CreateClassRequestDto dto);
    Task<List<ClassResponseDto>> BulkCreateClasses(long projectId, List<CreateClassRequestDto> classRequestDtos);
    Task<ClassResponseDto> UpdateClass(long projectId, long classId, UpdateClassRequestDto dto);
    Task<ClassResponseDto> GetClassInfo(long projectId, string className);
    Task<bool> DeleteClass(long projectId, long classId);
    Task<bool> ArchiveClass(long projectId, long classId);
    Task<bool> UnarchiveClass(long projectId, long classId);
    Task<List<ClassResponseDto>> GetClassesByName(long projectId, List<string> classNames);
}