using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IProjectBusiness
{
    Task<IEnumerable<ProjectResponseDto>> GetAllProjects();
    Task<ProjectResponseDto> GetProject(long projectId);
    Task<ProjectResponseDto> CreateProject(ProjectRequestDto dto);
    Task<ProjectResponseDto> UpdateProject(long projectId, ProjectRequestDto dto);
    Task<bool> DeleteProject(long projectId);
    Task<bool> ArchiveProject(long projectId);
}