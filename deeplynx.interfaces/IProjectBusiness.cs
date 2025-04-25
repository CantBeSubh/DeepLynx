using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IProjectBusiness
{
    Task<IEnumerable<ProjectDto>> GetAllProjects();
    Task<ProjectDto> GetProject(long projectId);
    Task<ProjectDto> CreateProject(ProjectDto dto);
    Task<ProjectDto> UpdateProject(long projectId, ProjectDto dto);
    Task<bool> DeleteProject(long projectId);
}