using Microsoft.EntityFrameworkCore;
using System.Transactions;

using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;

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
    /// Exceptions are handled per dependency for better logging and will be propagated up on error.
    /// Note: Downstream dependents on _context.Projects.Remove() should automatically be handled for us based on FK's.
    ///     We otherwise must handle our own soft-deletes.
    /// </summary>
    /// <param name="projectId">ID of the project to delete.</param>
    /// <param name="force">Boolean flag to force delete a project if true.</param>
    /// <returns>True boolean on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if project is not found.</exception>
    public async Task<bool> DeleteProject(long projectId, bool force = false)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null)
            throw new KeyNotFoundException("Project not found.");

        if (force)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
        else
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // We will define a list of lambda functions of our bulk deletes to sequentially iterate through
                // as to not block the thread of our lone database context.
                var softDeleteTasks = new List<Func<Task<bool>>>
                {
                    () => _tagBusiness.SoftDeleteAllTagsByProjectIdAsync(projectId),
                    () => _edgeMappingBusiness.SoftDeleteAllEdgeMappingsByProjectIdAsync(projectId),
                    () => _relationshipBusiness.SoftDeleteAllRelationshipsByProjectIdAsync(projectId),
                    () => _classBusiness.SoftDeleteAllClassesByProjectIdAsync(projectId),
                    () => _recordMappingBusiness.SoftDeleteAllRecordMappingsByProjectIdAsync(projectId),
                    () => _edgeBusiness.SoftDeleteAllEdgesByProjectIdAsync(projectId),
                    () => _dataSourceBusiness.SoftDeleteAllDataSourcesByProjectIdAsync(projectId),
                    () => _recordBusiness.SoftDeleteAllRecordsByProjectIdAsync(projectId),
                    () => _roleBusiness.SoftDeleteAllRolesByProjectIdAsync(projectId)
                };

                foreach (var task in softDeleteTasks)
                {
                    Console.WriteLine($"Executing task: {task.Method.Name}");
                    bool result = await task();
                    if (!result)
                    {
                        var methodName = task.Method.Name;
                        var offendingFunction = methodName.Substring(
                            "SoftDeleteAll".Length,
                            methodName.Length - "SoftDeleteAll".Length - "ByProjectIdAsync".Length
                        );
                        string message = $"An error occurred during deletion of project dependencies: {offendingFunction}.";
                        throw new ProjectDependencyDeletionException(message);
                    }
                }

                project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                _context.Projects.Update(project);

                await _context.SaveChangesAsync();
                transaction.Complete();
            }
        }
        
        return true;
    }
}
