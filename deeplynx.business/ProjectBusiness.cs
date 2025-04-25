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

    public async Task<IEnumerable<ProjectDto>> GetAllProjects()
    {
        var projects = await _context.Projects.ToListAsync();
        return projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Abbreviation = p.Abbreviation,
            CreatedBy = p.CreatedBy,
            CreatedAt = p.CreatedAt,
            ModifiedBy = p.ModifiedBy,
            ModifiedAt = p.ModifiedAt,
            DeletedAt = p.DeletedAt,
        });
    }

    public async Task<ProjectDto> GetProject(long projectId)
    {
        var project = await _context.Projects
            .Include(p => p.Classes)
            .Include(p => p.DataSources)
            .Include(p => p.EdgeParameters)
            .Include(p => p.Records)
            .Include(p => p.Relationships)
            .Include(p => p.Roles)
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new KeyNotFoundException("Project not found.");

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Abbreviation = project.Abbreviation,
            CreatedBy = project.CreatedBy,
            CreatedAt = project.CreatedAt,
            ModifiedBy = project.ModifiedBy,
            ModifiedAt = project.ModifiedAt,
            DeletedAt = project.DeletedAt,
        };
    }

    public async Task<ProjectDto> CreateProject(ProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Abbreviation = dto.Abbreviation,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow.ToLocalTime(),
            ModifiedBy = dto.ModifiedBy,
            ModifiedAt = DateTime.UtcNow.ToLocalTime(),
            DeletedAt = dto.DeletedAt
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        dto.Id = project.Id;
        return dto;
    }

    public async Task<ProjectDto> UpdateProject(long projectId, ProjectDto dto)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null)
            throw new KeyNotFoundException("Project not found.");

        project.Name = dto.Name;
        project.Abbreviation = dto.Abbreviation;
        project.CreatedBy = dto.CreatedBy;
        project.CreatedAt = dto.CreatedAt;
        project.ModifiedBy = dto.ModifiedBy;
        project.ModifiedAt = DateTime.UtcNow.ToLocalTime();
        project.DeletedAt = dto.DeletedAt;

        _context.Projects.Update(project);
        await _context.SaveChangesAsync();

        return dto;
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
