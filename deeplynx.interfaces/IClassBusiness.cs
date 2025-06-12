using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface IClassBusiness
{
    Task<IEnumerable<ClassResponseDto>> GetAllClasses(long projectId);
    Task<ClassResponseDto> GetClass(long projectId,  long classId);
    Task<ClassResponseDto> CreateClass(long projectId, ClassRequestDto dto);
    Task<ClassResponseDto> UpdateClass(long projectId, long classId, ClassRequestDto dto);
    Task<bool> DeleteClass(long projectId, long classId, bool force);
    Task<bool> BulkSoftDeleteClasses(Expression<Func<Class, bool>> predicate, IDbContextTransaction? transaction);
}