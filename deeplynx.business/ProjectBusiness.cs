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
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the record mapping operations.</param>
    /// <param name="tagBusiness">One of the downstream business layers used for cascading deletions.</param>
    /// <param name="edgeMappingBusiness">One of the downstream business layers used for cascading deletions.</param>
    /// <param name="relationshipBusiness">One of the downstream business layers used for cascading deletions.</param>
    /// <param name="classBusiness">One of the downstream business layers used for cascading deletions.</param>
    /// <param name="recordMappingBusiness">One of the downstream business layers used for cascading deletions.</param>
    /// <param name="edgeBusiness">One of the downstream business layers used for cascading deletions.</param>
    /// <param name="dataSourceBusiness">One of the downstream business layers used for cascading deletions.</param>
    /// <param name="recordBusiness">One of the downstream business layers used for cascading deletions.</param>
    /// <param name="roleBusiness">One of the downstream business layers used for cascading deletions.</param>
    public ProjectBusiness(
        DeeplynxContext context, 
        ITagBusiness tagBusiness, 
        IEdgeMappingBusiness edgeMappingBusiness,
        IRelationshipBusiness relationshipBusiness,
        IClassBusiness classBusiness,
        IRecordMappingBusiness recordMappingBusiness,
        IEdgeBusiness edgeBusiness,
        IDataSourceBusiness dataSourceBusiness,
        IRecordBusiness recordBusiness
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
    }

    /// <summary>
    /// Retrieves all projects
    /// </summary>
    /// <returns>A list of projects</returns>
    /// TODO: only list projects which the requesting user has access to once auth middleware is implemented
    public async Task<IEnumerable<ProjectResponseDto>> GetAllProjects()
    {
        var projects = await _context.Projects
            .Where(p => p.ArchivedAt == null).ToListAsync();

        return projects
            .Select(p => new ProjectResponseDto()
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Abbreviation = p.Abbreviation,
                CreatedBy = p.CreatedBy,
                CreatedAt = p.CreatedAt,
                ModifiedBy = p.ModifiedBy,
                ModifiedAt = p.ModifiedAt
            });
    }

    /// <summary>
    /// Retrieves a specific project by ID
    /// </summary>
    /// <param name="projectId">The ID by which to retrieve the project</param>
    /// <returns>The given project to return</returns>
    /// <exception cref="KeyNotFoundException">Returned if project not found</exception>
    public async Task<ProjectResponseDto> GetProject(long projectId)
    {
        var project = await _context.Projects
            .Where(p => p.Id == projectId && p.ArchivedAt == null)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Abbreviation = project.Abbreviation,
            CreatedBy = project.CreatedBy,
            CreatedAt = project.CreatedAt,
            ModifiedBy = project.ModifiedBy,
            ModifiedAt = project.ModifiedAt
        };
    }

    /// <summary>
    /// Creates a new project based on the data transfer object supplied.
    /// </summary>
    /// <param name="dto">A data transfer object with details on the new project to be created.</param>
    /// <returns>The new project which was just created.</returns>
    public async Task<ProjectResponseDto> CreateProject(ProjectRequestDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Abbreviation = dto.Abbreviation,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        }; 

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var classDto = new ClassRequestDto()
        {
            Name = "Timeseries"
        };

        await _classBusiness.CreateClass(project.Id, classDto);

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Abbreviation = project.Abbreviation,
            CreatedBy = project.CreatedBy,
            CreatedAt = project.CreatedAt
        };
    }

    /// <summary>
    /// Updates an existing project by ID
    /// </summary>
    /// <param name="projectId">The ID of the project to update</param>
    /// <param name="dto">A data transfer object with details on the project to be updated.</param>
    /// <returns>The project which was just updated.</returns>
    /// <exception cref="KeyNotFoundException">Returned if the project was not found.</exception>
    public async Task<ProjectResponseDto> UpdateProject(long projectId, ProjectRequestDto dto)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null || project.ArchivedAt is not null)
            throw new KeyNotFoundException("Project not found.");

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.Abbreviation = dto.Abbreviation;
        project.ModifiedBy = null; // TODO: handled in future by JWT.
        project.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        _context.Projects.Update(project);
        await _context.SaveChangesAsync();

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Abbreviation = project.Abbreviation,
            CreatedBy = project.CreatedBy,
            CreatedAt = project.CreatedAt,
            ModifiedBy = project.ModifiedBy,
            ModifiedAt = project.ModifiedAt
        };
    }

    /// <summary>
    /// Delete a project by id. This MUST also handle deletion of all project's downstream dependents.
    /// Exceptions are handled per dependency for better logging and will be propagated up on error.
    /// Note: Downstream dependents on force delete should automatically be handled for us based on FK's.
    ///     We otherwise must handle our own soft-deletes.
    /// </summary>
    /// <param name="projectId">ID of the project to delete.</param>
    /// <param name="force">Boolean flag to force delete a project if true.</param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if project is not found.</exception>
    /// <exception cref="DependencyDeletionException">Thrown if error during dependency deletions.</exception>
    /// TODO: We can maybe create a single timestamp to pass to functions ensuring all share exact archived_at time.
    public async Task<bool> DeleteProject(long projectId, bool force = false)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null || project.ArchivedAt is not null)
            throw new KeyNotFoundException("Project not found.");

        if (force)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
        else
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            
            // We will define a list of lambda functions of our bulk deletes to sequentially iterate through
            // as to not block the thread of our lone database context.
            // NOTE: transactions may be passed in to maintain downstream ACID compliance.
            var softDeleteTasks = new List<Func<Task<bool>>>
            {
                () => _tagBusiness.BulkSoftDeleteTags(t => t.ProjectId == projectId, transaction),
                () => _edgeMappingBusiness.BulkSoftDeleteEdgeMappings(em => em.ProjectId == projectId),
                () => _relationshipBusiness.BulkSoftDeleteRelationships(r => r.ProjectId == projectId, transaction),
                () => _classBusiness.BulkSoftDeleteClasses(c => c.ProjectId == projectId, transaction),
                () => _recordMappingBusiness.BulkSoftDeleteRecordMappings(m => m.ProjectId == projectId),
                () => _edgeBusiness.BulkSoftDeleteEdges(e => e.ProjectId == projectId),
                () => _dataSourceBusiness.BulkSoftDeleteDataSources(d => d.ProjectId == projectId, transaction),
                () => _recordBusiness.BulkSoftDeleteRecords(r => r.ProjectId == projectId, transaction)
            };

            // loop through tasks and trigger downstream deletions
            foreach (var task in softDeleteTasks)
            {
                bool result = await task();
                if (!result)
                {
                    // rollback the transaction and throw an error
                    await transaction.RollbackAsync();
                    throw new DependencyDeletionException(
                        "An error occurred during the deletion of downstream project dependants.");
                }
            }

            project.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.Projects.Update(project);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        
        return true;
    }
}
