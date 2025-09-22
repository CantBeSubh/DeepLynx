using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.helpers;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace deeplynx.business;
using DotNetEnv;
using System.Linq;

public class ProjectBusiness : IProjectBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;
    private readonly ILogger<ProjectBusiness> _logger;
    private readonly IClassBusiness _classBusiness;
    private readonly IDataSourceBusiness _dataSourceBusiness;
    private readonly IObjectStorageBusiness _objectStorageBusiness;
    private readonly ICacheBusiness _cacheBusiness;
    private readonly string ProjectsCacheKey = "projects";
    private readonly TimeSpan cacheTTL = TimeSpan.FromHours(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the project operations.</param>
    /// <param name="cacheBusiness">Used to cache project data</param>
    /// <param name="classBusiness">Used to create default classes automatically on project creation.</param>
    /// <param name="dataSourceBusiness">Used to create a default datasource on project creation.</param>
    /// <param name="eventBusiness">Used for logging events during create and update Operations.</param>
    public ProjectBusiness(
        DeeplynxContext context, ICacheBusiness cacheBusiness, ILogger<ProjectBusiness> logger,IClassBusiness classBusiness, 
        IDataSourceBusiness dataSourceBusiness, IObjectStorageBusiness objectStorageBusiness, IEventBusiness eventBusiness)
    {
        _context = context;
        _logger = logger;
        _classBusiness = classBusiness;
        _dataSourceBusiness = dataSourceBusiness;
        _objectStorageBusiness = objectStorageBusiness;
        _eventBusiness = eventBusiness;
        _cacheBusiness = cacheBusiness;
    }

    /// <summary>
    /// Retrieves all projects
    /// </summary>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result</param>
    /// <returns>A list of projects</returns>
    /// TODO: only list projects which the requesting user has access to once auth middleware is implemented
    public async Task<IEnumerable<ProjectResponseDto>> GetAllProjects(bool hideArchived)
    {
        // Check to see if projects are in Cache
        var cachedProjectList = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(ProjectsCacheKey);

        // If no projects are cached update the Cache
        if (cachedProjectList == null)
        {
            await RefreshProjectsCache();
            cachedProjectList = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(ProjectsCacheKey);
            if (cachedProjectList == null)
            {
                cachedProjectList = new List<ProjectResponseDto>();
            }
        }

        if (hideArchived)
        {
            cachedProjectList = cachedProjectList.Where(p => !p.IsArchived).ToList();
        }

        return cachedProjectList;
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
        var cachedProjectList = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(ProjectsCacheKey);
    
        // If no projects are cached update the Cache
        if (cachedProjectList == null || !cachedProjectList.Any())
        {
            await RefreshProjectsCache();
            cachedProjectList = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(ProjectsCacheKey);
        
            if (cachedProjectList == null)
            {
                cachedProjectList = new List<ProjectResponseDto>();
            }
        }
        
        var cachedProject = cachedProjectList.FirstOrDefault(p => p.Id == projectId);

        if (hideArchived && cachedProject != null)
        {
            if (cachedProject.IsArchived)
            {
                cachedProject = null;
            }
        }
        
        if (cachedProject == null)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }

        return cachedProject;
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
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null, // TODO: Implement user ID here when JWT tokens are ready
            OrganizationId = dto.OrganizationId,
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        
        var projectResponseDto = new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Abbreviation = project.Abbreviation,
            LastUpdatedBy = project.LastUpdatedBy,
            LastUpdatedAt = project.LastUpdatedAt,
            OrganizationId = project.OrganizationId
        };

        // Update the Project Cache List
        var cachedProjectList = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(ProjectsCacheKey);

        if (cachedProjectList == null)
        {
            cachedProjectList = new List<ProjectResponseDto>();
        }
        
        // add the new project to the project list and set the cache
        cachedProjectList.Add(projectResponseDto);
        await _cacheBusiness.SetAsync(ProjectsCacheKey, cachedProjectList, cacheTTL);
        
        // If project cache count differs from the database refresh it to match the database and return
        if (cachedProjectList.Count != _context.Projects.Count())
        {
            await RefreshProjectsCache();
        }

        // TODO: project config should determine whether to do this (true by default)
        // create built-in classes for Timeseries and Report
        var defaultClasses = new List<CreateClassRequestDto>
        {
            new CreateClassRequestDto { Name = "Timeseries" },
            new CreateClassRequestDto { Name = "Report" },
            new CreateClassRequestDto { Name = "File" }
        };
        await _classBusiness.BulkCreateClasses(project.Id, defaultClasses);

        // TODO: project config should determine whether to do this (true by default)
        // create default data source upon project creation
        var defaultDataSource = new CreateDataSourceRequestDto()
        {
            Name = "Default Data Source",
            Description = "This data source was created alongside the project for ease of use."
        };

        Env.Load("../.env");
        var defaultObjectStorageMethod = Environment.GetEnvironmentVariable("FILE_STORAGE_METHOD");

        var config = new JsonObject();
        if (defaultObjectStorageMethod == "filesystem")
        {
            var mountPath =
                Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ??
                throw new NullReferenceException($"Storage file path not set");
            config["mountPath"] = mountPath;
        }
        else if (defaultObjectStorageMethod == "azure_object")
        {
            var azureConnectionString =
                Environment.GetEnvironmentVariable("AZURE_OBJECT_CONNECTION_STRING") ??
                throw new NullReferenceException($"Azure connection string not set");
            config["azureConnectionString"] = azureConnectionString;
        }
        else if (defaultObjectStorageMethod == "aws_s3")
        {
            var awsConnectionString = Environment.GetEnvironmentVariable("AWS_S3_CONNECTION_STRING") ??
                                      throw new NullReferenceException($"AWS connection string not set");
            config["awsConnectionString"] = awsConnectionString;
        }
        else
        {
            throw new NullReferenceException(
                $"Unknown object storage method, make sure your environment variables are correctly set");
        }

        var objectStorageRequestDto = new CreateObjectStorageRequestDto
        {
            Name = "Instance Default",
            Config = config
        };
        await _objectStorageBusiness.CreateObjectStorage(project.Id, objectStorageRequestDto, true);

        await _dataSourceBusiness.CreateDataSource(project.Id, defaultDataSource, true);

        // Log create Project event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = project.Id,
            Operation = "create",
            EntityType = "project",
            EntityId = project.Id,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new { project.Name }),
            LastUpdatedBy = "" // TODO: add username when JWT are implemented
        });

        return projectResponseDto;
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

        if (project == null || project.IsArchived)
        { 
            throw new KeyNotFoundException("Project not found.");
        }

        project.Name = dto.Name ?? project.Name;
        project.Description = dto.Description ?? project.Description;
        project.Abbreviation = dto.Abbreviation ?? project.Abbreviation;
        project.LastUpdatedBy = null; // TODO: handled in future by JWT.
        project.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    
        // Log update Project event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = project.Id,
            Operation = "update",
            EntityType = "project",
            EntityId = project.Id,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new {project.Name}),
            LastUpdatedBy = "" // TODO: add username when JWT are implemented
        });

        var updatedProject = new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Abbreviation = project.Abbreviation,
            IsArchived = project.IsArchived,
            LastUpdatedAt = project.LastUpdatedAt,
            LastUpdatedBy = project.LastUpdatedBy,
            OrganizationId = project.OrganizationId
        };

        // Update the Project Cache List
        var cachedProjectList = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(ProjectsCacheKey);

        // If cache list is empty, refresh it to match the database and return
        if (cachedProjectList == null)
        {
            await RefreshProjectsCache();
            return updatedProject;
        }

        // If cache exists, update the project in the list
        var projectIndex = cachedProjectList.FindIndex(p => p.Id == updatedProject.Id);
        if (projectIndex != -1)
        {
            cachedProjectList[projectIndex] = updatedProject;
        }

        // Set the updated list back to the cache
        await _cacheBusiness.SetAsync(ProjectsCacheKey, cachedProjectList, cacheTTL);

        return updatedProject;
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
        
        // Update the Project Cache List
        var cachedProjectList = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(ProjectsCacheKey);

        // If cache list is empty, refresh it to match the database and return
        if (cachedProjectList == null)
        {
            await RefreshProjectsCache();
            return true;
        }
        
        var projectIndex = cachedProjectList.FindIndex(p => p.Id == projectId);
        if (projectIndex != -1)
        {
            cachedProjectList.RemoveAt(projectIndex);
        }
        
        await _cacheBusiness.SetAsync(ProjectsCacheKey, cachedProjectList, cacheTTL);
        
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

        if (project == null || project.IsArchived)
            throw new KeyNotFoundException("Project not found.");

        // set lastUpdatedAt timestamp
        var lastUpdatedAt = DateTime.UtcNow;

        // run archive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the archive project procedure, which archives this project
                // and all child objects with project_id as a foreign key
                var archived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.archive_project({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)",
                    projectId, lastUpdatedAt
                );

                if (archived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException(
                        $"unable to archive project {projectId} or its downstream dependents.");
                }
                
                await transaction.CommitAsync();
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException(
                    $"unable to archive project {projectId} or its downstream dependents: {exc}");
            }
        }

        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "archive",
            EntityType = "project",
            EntityId = project.Id,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new { project.Name }),
            LastUpdatedBy = "" // TODO: add username when JWT are implemented
        });

        var projectResponse = new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Abbreviation = project.Abbreviation,
            LastUpdatedAt = project.LastUpdatedAt,
            LastUpdatedBy = project.LastUpdatedBy,
            IsArchived = project.IsArchived,
        };
        
        // Update the Project Cache List
        var cachedProjectList = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(ProjectsCacheKey);

        // If cache list is empty, refresh it to match the database and return
        if (cachedProjectList == null)
        {
            await RefreshProjectsCache();
            return true;
        }

        // If cache exists, update the project in the list
        var projectIndex = cachedProjectList.FindIndex(p => p.Id == projectResponse.Id);
        if (projectIndex != -1)
        {
            cachedProjectList[projectIndex] = projectResponse;
        }

        // Set the updated list back to the cache
        await _cacheBusiness.SetAsync(ProjectsCacheKey, cachedProjectList, cacheTTL);
        
        return true;
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

        if (project == null || !project.IsArchived)
            throw new KeyNotFoundException("Project not found or is not archived.");

        // set lastUpdatedAt timestamp
        var lastUpdatedAt = DateTime.UtcNow;

        // run unarchive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the unarchive project procedure, which unarchives this project
                // and all child objects with project_id as a foreign key
                var unarchived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.unarchive_project({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)",
                    projectId, lastUpdatedAt
                );

                if (unarchived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException(
                        $"unable to unarchive project {projectId} or its downstream dependents.");
                }
                
                await transaction.CommitAsync();
                
                var projectResponse = new ProjectResponseDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Abbreviation = project.Abbreviation,
                    LastUpdatedAt = project.LastUpdatedAt,
                    LastUpdatedBy = project.LastUpdatedBy,
                    IsArchived = project.IsArchived,
                };
                
                // Update the Project Cache List
                var cachedProjectList = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(ProjectsCacheKey);

                // If cache list is empty, refresh it to match the database and return
                if (cachedProjectList == null)
                {
                    await RefreshProjectsCache();
                    return true;
                }

                // If cache exists, update the project in the list
                var projectIndex = cachedProjectList.FindIndex(p => p.Id == projectResponse.Id);
                if (projectIndex != -1)
                {
                    cachedProjectList[projectIndex] = projectResponse;
                }

                // Set the updated list back to the cache
                await _cacheBusiness.SetAsync(ProjectsCacheKey, cachedProjectList, cacheTTL);
                
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException(
                    $"unable to unarchive project {projectId} or its downstream dependents: {exc}");
            }
        }

        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "unarchive",
            EntityType = "project",
            EntityId = project.Id,
            DataSourceId = null,
            Properties = JsonSerializer.Serialize(new { project.Name }),
            LastUpdatedBy = "" // TODO: add username when JWT are implemented
        });
        
        return true;
    }

    /// <summary>
    /// Retrieves project stats
    /// </summary>
    /// <returns>A list of project stats</returns>
    public async Task<ProjectStatResponseDto> GetProjectStats(long projectId)
    {
        //classes": number, “dataRecords”: number, “connections”: number 
        var classes = _context.Classes
            .Where(p =>  !p.IsArchived  && p.ProjectId == projectId).Count();
        var records = _context.Records
            .Where(p => !p.IsArchived  && p.ProjectId == projectId).Count();
        var datasources = _context.DataSources
            .Where(p => !p.IsArchived && p.ProjectId == projectId).Count();
        
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
            recordQuery = recordQuery.Where(r => !r.IsArchived);
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
                ClassName = r.ClassName,
                DataSourceId = r.DataSourceId,
                ProjectId = r.ProjectId,
                LastUpdatedAt = r.LastUpdatedAt,
                LastUpdatedBy = r.LastUpdatedBy,
                IsArchived = r.IsArchived,
                Tags = r.Tags
            });
    }
    
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    
    private async Task<bool> RefreshProjectsCache()
    {
        var dbProjects = await _context.Projects.ToListAsync();
        var projectResponseDtoList = MapProjectsToResponseDto(dbProjects);
        await _cacheBusiness.SetAsync(ProjectsCacheKey, projectResponseDtoList, cacheTTL);
        return true;
    }
    
    private List<ProjectResponseDto> MapProjectsToResponseDto(List<Project> projects)
    {
        return projects.Select(p => new ProjectResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Abbreviation = p.Abbreviation,
            LastUpdatedBy = p.LastUpdatedBy,
            LastUpdatedAt = p.LastUpdatedAt,
            IsArchived = p.IsArchived,
            OrganizationId =  p.OrganizationId
        }).ToList();
    }
    
}
