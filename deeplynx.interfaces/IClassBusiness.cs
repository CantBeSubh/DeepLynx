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
    Task<ClassResponseDto> GetClassInfo(string projectId, string className);
    Task<bool> DeleteClass(long projectId, long classId);
    Task<bool> ArchiveClass(long projectId, long classId);
}