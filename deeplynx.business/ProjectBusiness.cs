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
    private readonly IEdgeMappingBusiness _edgeMappingBusiness;
    private readonly IRelationshipBusiness _relationshipBusiness;
    private readonly IClassBusiness _classBusiness;
    private readonly IRecordMappingBusiness _recordMappingBusiness;
    private readonly IEdgeBusiness _edgeBusiness;
    private readonly IDataSourceBusiness _dataSourceBusiness;
    
    public ProjectBusiness(
        DeeplynxContext context, 
        ITagBusiness tagBusiness, 
        IEdgeMappingBusiness edgeMappingBusiness,
        IRelationshipBusiness relationshipBusiness,
        IClassBusiness classBusiness,
        IRecordMappingBusiness recordMappingBusiness,
        IEdgeBusiness edgeBusiness,
        IDataSourceBusiness dataSourceBusiness
        )
    {
        _context = context;
        _tagBusiness = tagBusiness;
        _edgeMappingBusiness = edgeMappingBusiness;
        _relationshipBusiness = relationshipBusiness;
        _classBusiness = classBusiness;
        _recordMappingBusiness = recordMappingBusiness;
        _edgeBusiness = edgeBusiness;
        _dataSourceBusiness = dataSourceBusiness;
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
    /// NOTE: This takes in a force boolean for force support, however
    ///     the project will currently only be soft-deleted.
    /// Delete a project by id. This MUST also handle deletion of all project's downstream dependents.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="force"></param>
    /// <returns>True boolean on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<bool> DeleteProject(long projectId, bool force = false)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null)
            throw new KeyNotFoundException("Project not found.");

        if (force)
        {
            throw new NotSupportedException("Deeplynx does not support forced project deletion at this time. Check back later.");
            // Note: Downstream dependents should be handled by below line based on FK's. Added force protection for dev safety.
            //_context.Projects.Remove(project);
        }
        else
        {
            await _tagBusiness.SoftDeleteAllTagsByProjectIdAsync(projectId);
            await _edgeMappingBusiness.SoftDeleteAllEdgeMappingsByProjectIdAsync(projectId);
            await _relationshipBusiness.SoftDeleteAllRelationshipsByProjectIdAsync(projectId);
            await _classBusiness.SoftDeleteAllClassesByProjectIdAsync(projectId);
            await _recordMappingBusiness.SoftDeleteAllRecordMappingsByProjectIdAsync(projectId);
            await _edgeBusiness.SoftDeleteAllEdgesByProjectIdAsync(projectId);
            await _dataSourceBusiness.SoftDeleteAllDataSourcesByProjectIdAsync(projectId);
            project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.Projects.Update(project);
        }
        await _context.SaveChangesAsync();
        return true;
    }
}
