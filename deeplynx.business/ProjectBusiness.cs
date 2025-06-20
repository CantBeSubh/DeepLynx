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
    /// Delete a project by id.
    /// </summary>
    /// <param name="projectId">ID of the project to delete.</param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if project is not found.</exception>
    public async Task<bool> DeleteProject(long projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null || project.ArchivedAt is not null)
            throw new KeyNotFoundException("Project not found.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return true;
    }
    
    /// <summary>
    /// Archive (soft delete) a project by id. This also archives downstream dependents.
    /// </summary>
    /// <param name="projectId">ID of the project to delete.</param>
    /// <returns>Boolean true on successful deletion.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if project is not found.</exception>
    public async Task<bool> ArchiveProject(long projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null || project.ArchivedAt is not null)
            throw new KeyNotFoundException("Project not found.");

        // set archivedAt timestamp
        var archivedAt = DateTime.UtcNow;

        // run archive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the archive project procedure, which archives this project
                // and all child objects with project_id as a foreign key
                var archived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.archive_project({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)", projectId, archivedAt);

                if (archived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException($"unable to archive project {projectId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException($"unable to archive project {projectId} or its downstream dependents: {exc}");
            }
        }
    }
}
