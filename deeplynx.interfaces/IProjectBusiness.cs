using deeplynx.datalayer.Models;

namespace deeplynx.interfaces;

public interface IProjectBusiness
{
    Task<IEnumerable<Project>> GetAllProjects();
    Task<Project> GetProject(long projectId);
    Task<Project> CreateProject(Project project);
    Task<Project> UpdateProject(long projectId, Project project);
    Task<bool> DeleteProject(long projectId);
}