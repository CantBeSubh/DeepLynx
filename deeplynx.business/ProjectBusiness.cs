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
    private readonly IRecordBusiness _recordBusiness;
    private readonly IRoleBusiness _roleBusiness;
    
    public ProjectBusiness(
        DeeplynxContext context, 
        ITagBusiness tagBusiness, 
        IEdgeMappingBusiness edgeMappingBusiness,
        IRelationshipBusiness relationshipBusiness,
        IClassBusiness classBusiness,
        IRecordMappingBusiness recordMappingBusiness,
        IEdgeBusiness edgeBusiness,
        IDataSourceBusiness dataSourceBusiness,
        IRecordBusiness recordBusiness,
        IRoleBusiness roleBusiness
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
        _recordBusiness = recordBusiness;
        _roleBusiness = roleBusiness;
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
    /// Delete a project by id. This MUST also handle deletion of all project's downstream dependents.
    /// Note: Downstream dependents on _context.Projects.Remove() should automatically be handled for us based on FK's.
    ///     We otherwise must handle our own soft-deletes.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="force">Boolean flag to force delete a project if true.</param>
    /// <returns>True boolean on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<bool> DeleteProject(long projectId, bool force = false)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found.");

            if (force)
            {
                _context.Projects.Remove(project);
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
                await _recordBusiness.SoftDeleteAllRecordsByProjectIdAsync(projectId);
                await _roleBusiness.SoftDeleteAllRolesByProjectIdAsync(projectId);
                project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                _context.Projects.Update(project);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            var message = $"An error occurred while deleting project {projectId}: {keyNotFoundException.Message}";
            NLog.LogManager.GetCurrentClassLogger().Error(keyNotFoundException);
            throw new KeyNotFoundException(message);
        }
        catch (DbUpdateException dbUpdateException)
        {
            var message = $"An error occurred while updating the database: {dbUpdateException.Message}";
            NLog.LogManager.GetCurrentClassLogger().Error(dbUpdateException);
            throw new DbUpdateException(message);
        }
        catch (Exception exception)
        {
            var message = $"An unexpected error occurred: {exception.Message}";
            NLog.LogManager.GetCurrentClassLogger().Error(exception);
            throw new Exception(message);
        }
    }
}
