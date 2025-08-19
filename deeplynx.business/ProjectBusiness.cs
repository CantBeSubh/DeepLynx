using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.helpers;
using Serilog;
using Microsoft.Extensions.Logging;

namespace deeplynx.business;

public class ProjectBusiness : IProjectBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ILogger<ProjectBusiness> _logger;

    private readonly IClassBusiness _classBusiness;
    private readonly IDataSourceBusiness _dataSourceBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the record mapping operations.</param>
    /// <param name="classBusiness">Used to create default classes automatically on project creation.</param>
    /// <param name="dataSourceBusiness">Used to create a default datasource on project creation.</param>
    public ProjectBusiness(DeeplynxContext context, ILogger<ProjectBusiness> logger,IClassBusiness classBusiness, IDataSourceBusiness dataSourceBusiness)
    {
        _context = context;
        _logger = logger;
        _classBusiness = classBusiness;
        _dataSourceBusiness = dataSourceBusiness;
    }

    /// <summary>
    /// Retrieves all projects
    /// </summary>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result</param>
    /// <returns>A list of projects</returns>
    /// TODO: only list projects which the requesting user has access to once auth middleware is implemented
    public async Task<IEnumerable<ProjectResponseDto>> GetAllProjects(bool hideArchived)
    {
        var projects = await _context.Projects.ToListAsync();

        if (hideArchived)
        {
            projects = projects.Where(p => p.ArchivedAt == null).ToList();
        }

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
                ModifiedAt = p.ModifiedAt,
                ArchivedAt = p.ArchivedAt,
            });
    }

    /// <summary>
    /// Retrieves a specific project by ID
    /// </summary>
    /// <param name="projectId">The ID by which to retrieve the project</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result</param>
    /// <returns>The given project to return</returns>
    /// <exception cref="KeyNotFoundException">Returned if project not found or is archived</exception>
    public async Task<ProjectResponseDto> GetProject(long projectId, bool hideArchived)
    {
        var project = await _context.Projects
            .Where(p => p.Id == projectId)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }
        
        if (hideArchived && project.ArchivedAt != null)
        {
            throw new KeyNotFoundException($"Project with id {projectId} is archived");
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
            ModifiedAt = project.ModifiedAt,
            ArchivedAt = project.ArchivedAt,
        };
    }

    /// <summary>
    /// Creates a new project based on the data transfer object supplied.
    /// </summary>
    /// <param name="dto">A data transfer object with details on the new project to be created.</param>
    /// <returns>The new project which was just created.</returns>
    public async Task<ProjectResponseDto> CreateProject(CreateProjectRequestDto dto)
    {
        ValidationHelper.ValidateModel(dto);
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

        // TODO: project config should determine whether to do this (true by default)
        // create built-in classes for Timeseries and Report
        var defaultClasses = new List<CreateClassRequestDto>
        {
            new CreateClassRequestDto {Name = "Timeseries"},
            new CreateClassRequestDto {Name = "Report"},
        };
        await _classBusiness.BulkCreateClasses(project.Id, defaultClasses);
        
        // TODO: project config should determine whether to do this (true by default)
        // create default data source upon project creation
        var defaultDataSource = new CreateDataSourceRequestDto()
        {
            Name = "Default Data Source",
            Description = "This data source was created alongside the project for ease of use."
        };
        await _dataSourceBusiness.CreateDataSource(project.Id, defaultDataSource);

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
    public async Task<ProjectResponseDto> UpdateProject(long projectId, UpdateProjectRequestDto dto)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null || project.ArchivedAt is not null)
            throw new KeyNotFoundException("Project not found.");

        project.Name = dto.Name ?? project.Name;
        project.Description = dto.Description ?? project.Description;
        project.Abbreviation = dto.Abbreviation ?? project.Abbreviation;
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

        if (project == null)
            throw new KeyNotFoundException($"Project with id {projectId} not found.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Archive (soft delete) a project by id. This also archives downstream dependents.
    /// </summary>
    /// <param name="projectId">ID of the project to archive.</param>
    /// <returns>Boolean true on successful archival.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if project is not found.</exception>
    /// <exception cref="DependencyDeletionException">Thrown if archival fails.</exception>
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
                    "CALL deeplynx.archive_project({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)", projectId,
                    archivedAt);

                if (archived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException(
                        $"unable to archive project {projectId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException(
                    $"unable to archive project {projectId} or its downstream dependents: {exc}");
            }
        }
    }
    
    /// <summary>
    /// Unarchive a project by id. This also unarchives downstream dependents.
    /// </summary>
    /// <param name="projectId">ID of the project to unarchive.</param>
    /// <returns>Boolean true when successfully unarchived.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if project is not found.</exception>
    /// <exception cref="DependencyDeletionException">Thrown if unarchive action fails.</exception>
    public async Task<bool> UnarchiveProject(long projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);

        if (project == null || project.ArchivedAt is null)
            throw new KeyNotFoundException("Project not found or is not archived.");

        // run unarchive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the unarchive project procedure, which unarchives this project
                // and all child objects with project_id as a foreign key
                var unarchived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.unarchive_project({0}::INTEGER)", projectId);

                if (unarchived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException(
                        $"unable to unarchive project {projectId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException(
                    $"unable to unarchive project {projectId} or its downstream dependents: {exc}");
            }
        }
    }

    /// <summary>
    /// Retrieves project stats
    /// </summary>
    /// <returns>A list of project stats</returns>
    public async Task<ProjectStatResponseDto> GetProjectStats(long projectId)
    {
        //classes”: number, “dataRecords”: number, “connections”: number 
        var classes = _context.Classes
            .Where(p => p.ArchivedAt == null && p.ProjectId == projectId).Count();
        var records = _context.Records
            .Where(p => p.ArchivedAt == null && p.ProjectId == projectId).Count();
        var datasources = _context.DataSources
            .Where(p => p.ArchivedAt == null && p.ProjectId == projectId).Count();
        
        var response = new ProjectStatResponseDto()
            {
               classes = classes,
               records = records,
               datasources =  datasources
            };
        return response;
    }
    
    /// <summary>
    /// Retrieves all records for multiple projects.
    /// </summary>
    /// <param name="projects">Array of project ids whose records are to be retrieved</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
    /// <returns>A list of records based on the applied filters.</returns>
    public async Task<IEnumerable<HistoricalRecordResponseDto>> GetMultiProjectRecords(
        long[] projects, bool hideArchived)
    {
        var recordQuery = _context.HistoricalRecords
            .Where(r => projects.Contains(r.ProjectId));

        if (hideArchived)
        {
            recordQuery = recordQuery.Where(r => r.ArchivedAt == null);
        }
        
        var records = await recordQuery
            .GroupBy(e => e.RecordId)
            .Select(g => g.OrderByDescending(r => r.LastUpdatedAt).FirstOrDefault())
            .ToListAsync();

        return records
            .Select(r => new HistoricalRecordResponseDto()
            {
                Id = r.RecordId,
                Description = r.Description,
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                ClassId = r.ClassId,
                DataSourceId = r.DataSourceId,
                ProjectId = r.ProjectId,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                ModifiedBy = r.ModifiedBy,
                ModifiedAt = r.ModifiedAt,
                ArchivedAt = r.ArchivedAt,
                Tags = r.Tags
            });
    }
    
}
