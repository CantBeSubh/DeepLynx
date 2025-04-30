using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
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

    public async Task<Project> CreateProject(ProjectRequestDto project)
    {
        var newProject = new Project
        {
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null, //TODO: Will add once JWTs work
            Name = project.Name,
            Abbreviation = project.Abbreviation,
        }; 

        _context.Projects.Add(newProject);
        await _context.SaveChangesAsync();

        return newProject;
    }

    public async Task<Project> UpdateProject(long projectId, ProjectRequestDto project)
    {
        var existingProject = await _context.Projects.FindAsync(projectId);

        if (existingProject == null)
            throw new KeyNotFoundException("Project not found.");

        existingProject.Name = project.Name;
        existingProject.Abbreviation = project.Abbreviation;
        existingProject.ModifiedBy = null; //TODO: Will add once JWTS work
        existingProject.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

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
