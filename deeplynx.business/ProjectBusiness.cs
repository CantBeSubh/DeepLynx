using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class ProjectBusiness : IProjectBusiness
{
    private readonly DeeplynxContext _context;

    public ProjectBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllProjects()
    {
        return await _context.Projects.ToListAsync();
    }

    public async Task<Project> GetProject(long projectId)
    {
        return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId)
            ?? throw new KeyNotFoundException("Project not found.");
    }

    public async Task<Project> CreateProject(Project project)
    {
        project.CreatedAt = DateTime.UtcNow.ToLocalTime();
        project.ModifiedAt = DateTime.UtcNow.ToLocalTime();

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return project;
    }

    public async Task<Project> UpdateProject(long projectId, Project project)
    {
        var existingProject = await _context.Projects.FindAsync(projectId);

        if (existingProject == null)
            throw new KeyNotFoundException("Project not found.");

        existingProject.Name = project.Name;
        existingProject.Abbreviation = project.Abbreviation;
        existingProject.CreatedBy = project.CreatedBy;
        existingProject.CreatedAt = project.CreatedAt;
        existingProject.ModifiedBy = project.ModifiedBy;
        existingProject.ModifiedAt = DateTime.UtcNow.ToLocalTime();
        existingProject.DeletedAt = project.DeletedAt;

        _context.Projects.Update(existingProject);
        await _context.SaveChangesAsync();

        return existingProject;
    }

    public async Task<bool> DeleteProject(long projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null)
            throw new KeyNotFoundException("Project not found.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }
}
