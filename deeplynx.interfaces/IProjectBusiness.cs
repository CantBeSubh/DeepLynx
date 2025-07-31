using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IProjectBusiness
{
    Task<IEnumerable<ProjectResponseDto>> GetAllProjects(bool hideArchived);
    Task<ProjectResponseDto> GetProject(long projectId, bool hideArchived);
    Task<ProjectResponseDto> CreateProject(CreateProjectRequestDto dto);
    Task<ProjectResponseDto> UpdateProject(long projectId, UpdateProjectRequestDto dto);
    Task<bool> DeleteProject(long projectId);
    Task<bool> ArchiveProject(long projectId);
    Task<bool> UnarchiveProject(long projectId);
    Task<ProjectStatResponseDto> GetProjectStats(long projectId);
    Task<IEnumerable<RecordResponseDto>> GetMultiProjectRecords(long[] projects, bool hideArchived); 
}