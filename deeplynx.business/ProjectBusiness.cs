using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class ProjectBusiness : IProjectBusiness
{
    private readonly DeeplynxContext _context;
    
    /// Note: The following dependencies are used exclusively for their respective bulk soft delete functions.
    private readonly ITagBusiness _tagBusiness;

    public ProjectBusiness(DeeplynxContext context, ITagBusiness tagBusiness)
    {
        _context = context;
        _tagBusiness = tagBusiness;
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

    /// <summary>
    /// NOTE: This takes in a force boolean for future force support however
    ///     the project will currently only be soft-deleted no matter what.
    ///     Note the commented out force removal thus the same if/else result.
    /// Delete a project by id. This MUST also handle deletion of ALL downstream dependents.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="force"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<bool> DeleteProject(long projectId, bool force = false)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null)
            throw new KeyNotFoundException("Project not found.");

        // Note: If force deleting, downstream dependents must be handled first.
        if (force)
        {
            project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            // The actual force delete is the line below and should remain the final line.
            // _context.Projects.Remove(project);
        }
        else
        {
            await _tagBusiness.SoftDeleteAllTagsByProjectIdAsync(projectId);
            project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.Projects.Update(project);
        }
        await _context.SaveChangesAsync();
        return true;
    }
}
