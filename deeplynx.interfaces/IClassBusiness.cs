using deeplynx.models;

namespace deeplynx.interfaces;

public interface IClassBusiness
{
    Task<IEnumerable<ClassResponseDto>> GetAllClasses(long projectId, bool hideArchived);
    Task<ClassResponseDto> GetClass(long projectId, long classId, bool hideArchived);
    Task<ClassResponseDto> CreateClass(long projectId, ClassRequestDto dto);
    Task<List<ClassResponseDto>> BulkCreateClass(long projectId, List<ClassRequestDto> classRequestDtos);
    Task<ClassResponseDto> UpdateClass(long projectId, long classId, ClassRequestDto dto);
    Task<ClassResponseDto> GetClassInfo(long projectId, string className);
    Task<bool> DeleteClass(long projectId, long classId);
    Task<bool> ArchiveClass(long projectId, long classId);
    Task<bool> UnarchiveClass(long projectId, long classId);
}