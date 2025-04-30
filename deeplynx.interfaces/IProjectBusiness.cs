using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IProjectBusiness
{
    Task<IEnumerable<Project>> GetAllProjects();
    Task<Project> GetProject(long projectId);
    Task<Project> CreateProject(ProjectRequestDto project);
    Task<Project> UpdateProject(long projectId, ProjectRequestDto project);
    Task<bool> DeleteProject(long projectId);
}