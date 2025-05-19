using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IClassBusiness
{
    Task<IEnumerable<Class>> GetAllClasses(long projectId);
    Task<Class> GetClass(long projectId,  long classId);
    Task<Class> CreateClass(long projectId, ClassRequestDto dto);
    Task<Class> UpdateClass(long projectId, long classId, ClassRequestDto dto);
    Task<bool> DeleteClass(long projectId, long classId);
    Task<bool> SoftDeleteAllClassesByProjectIdAsync(long projectId);
}