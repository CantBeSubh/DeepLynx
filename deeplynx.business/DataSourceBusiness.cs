using System.Data;
using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.helpers;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace deeplynx.business
{
    public class DataSourceBusiness : IDataSourceBusiness
    {
        private readonly DeeplynxContext _context;
        private readonly ICacheBusiness _cacheBusiness;
    
        // dependants used to trigger downstream soft deletes
        private readonly IEdgeBusiness _edgeBusiness;
        private readonly IRecordBusiness _recordBusiness;
        private readonly IEventBusiness _eventBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceBusiness"/> class.
        /// </summary>
        /// <param name="context">The database context used for the data source operations.</param>
        /// <param name="cacheBusiness">Used to access cache operations</param>
        /// <param name="edgeBusiness">Passed in context for downstream edge objects.</param>
        /// <param name="recordBusiness">Passed in context for downstream record objects.</param>
        /// <param name="eventBusiness">Used for logging events during create, update, and delete Operations.</param>
        public DataSourceBusiness(
            DeeplynxContext context,
            ICacheBusiness cacheBusiness,
            IEdgeBusiness edgeBusiness,
            IRecordBusiness recordBusiness,
            IEventBusiness eventBusiness
            )
        {
            _context = context;
            _edgeBusiness = edgeBusiness;
            _recordBusiness = recordBusiness;
            _eventBusiness = eventBusiness;
            _cacheBusiness = cacheBusiness;
        }

        /// <summary>
        /// Retrieves all data sources for a specific project.
        /// </summary>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="projectId">The ID of the project whose data sources are to be retrieved</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result</param>
        /// <returns>A list of data sources within the given project.</returns>
        public async Task<List<DataSourceResponseDto>> GetAllDataSources(long organizationId, long projectId, bool hideArchived)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);
            
            var dataSources = await _context.DataSources
                .Where(d => d.ProjectId == projectId).ToListAsync();

            if (hideArchived)
            {
                dataSources = dataSources.Where(d =>  !d.IsArchived).ToList();
            }
            
            return dataSources
                .Select(d => new DataSourceResponseDto()
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    Default = d.Default,
                    Abbreviation = d.Abbreviation,
                    Type = d.Type,
                    BaseUri = d.BaseUri,
                    // return empty object for config if null
                    Config = JsonNode.Parse(d.Config ?? "{}") as JsonObject,
                    ProjectId = d.ProjectId,
                    LastUpdatedAt = d.LastUpdatedAt,
                    LastUpdatedBy = d.LastUpdatedBy,
                    IsArchived = d.IsArchived,

                }).ToList();
        }

        /// <summary>
        /// Retrieve a specific data source by its ID
        /// </summary>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="datasourceId">The ID of the data source</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result</param>
        /// <returns>The data source in question</returns>
        /// <exception cref="KeyNotFoundException">Returned if the data source is not found or is archived</exception>
        public async Task<DataSourceResponseDto> GetDataSource(long organizationId, long projectId, long datasourceId, bool hideArchived)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);
            var dataSource = await _context.DataSources
                .Where(d => d.ProjectId == projectId && d.Id == datasourceId)
                .FirstOrDefaultAsync();

            if (dataSource == null || dataSource.ProjectId != projectId)
            {
                throw new KeyNotFoundException($"Data Source with id {datasourceId} not found");
            }
            
            if (hideArchived && dataSource.IsArchived)
            {
                throw new KeyNotFoundException($"Data Source with id {datasourceId} is archived");
            }

            return new DataSourceResponseDto
            {
                Id = dataSource.Id,
                Name = dataSource.Name,
                Description = dataSource.Description,
                Default = dataSource.Default,
                Abbreviation = dataSource.Abbreviation,
                Type = dataSource.Type,
                BaseUri = dataSource.BaseUri,
                // return empty object for config if null
                Config = JsonNode.Parse(dataSource.Config ?? "{}") as JsonObject,
                ProjectId = dataSource.ProjectId,
                LastUpdatedAt = dataSource.LastUpdatedAt,
                LastUpdatedBy = dataSource.LastUpdatedBy,
                IsArchived = dataSource.IsArchived
            };
        }
        
        /// <summary>
        /// Retrieve a project's default data source.
        /// </summary>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <returns>The data source in question</returns>
        /// <exception cref="KeyNotFoundException">Returned if the data source is not found or is archived</exception>
        public async Task<DataSourceResponseDto> GetDefaultDataSource(long organizationId, long projectId)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
            var dataSource = await _context.DataSources
                .Where(d => d.ProjectId == projectId && d.Default == true && !d.IsArchived)
                .FirstOrDefaultAsync();

            if (dataSource == null || dataSource.ProjectId != projectId)
            {
                throw new KeyNotFoundException($"Default data source for project {projectId} not found");
            }

            return new DataSourceResponseDto
            {
                Id = dataSource.Id,
                Name = dataSource.Name,
                Description = dataSource.Description,
                Default = dataSource.Default,
                Abbreviation = dataSource.Abbreviation,
                Type = dataSource.Type,
                BaseUri = dataSource.BaseUri,
                // return empty object for config if null
                Config = JsonNode.Parse(dataSource.Config ?? "{}") as JsonObject,
                ProjectId = dataSource.ProjectId,
                LastUpdatedAt = dataSource.LastUpdatedAt,
                LastUpdatedBy = dataSource.LastUpdatedBy,
                IsArchived = dataSource.IsArchived,
            };
        }

        /// <summary>
        /// Asynchronously creates a new data source for a specified project.
        /// </summary>
        /// <param name="currentUserId">ID of the User executing this method.</param>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="dto">The data transfer object containing data source details</param>
        /// <returns>The created data source.</returns>
        public async Task<DataSourceResponseDto> CreateDataSource(long currentUserId, long organizationId, long projectId, CreateDataSourceRequestDto dto, bool makeDefault = false)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var dataSource = new DataSource
            {
                Name = dto.Name,
                ProjectId = projectId,
                Description = dto.Description,
                Default = makeDefault,
                BaseUri = dto.BaseUri,
                Abbreviation = dto.Abbreviation,
                Config = dto.Config?.ToString(),
                Type = dto.Type,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = currentUserId,
                IsArchived = false
            };

            await _context.DataSources.AddAsync(dataSource);
            
            if (makeDefault)
                await MakePreviousDefaultsFalse(currentUserId, organizationId, projectId, dataSource.Id);
            
            await _context.SaveChangesAsync();
            
            // Log DataSource Create Event
            await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
            {
                ProjectId = projectId,
                Operation = "create",
                EntityType = "data_source",
                EntityId = dataSource.Id,
                EntityName = dataSource.Name,
                DataSourceId = null,
                Properties = JsonSerializer.Serialize(new {dataSource.Name}),
            });

            return new DataSourceResponseDto
            {
                Id = dataSource.Id,
                Name = dataSource.Name,
                Description = dataSource.Description,
                Default = dataSource.Default,
                Abbreviation = dataSource.Abbreviation,
                Type = dataSource.Type,
                BaseUri = dataSource.BaseUri,
                // return empty object for config if null
                Config = JsonNode.Parse(dataSource.Config ?? "{}") as JsonObject,
                ProjectId = dataSource.ProjectId,
                LastUpdatedAt = dataSource.LastUpdatedAt,
                LastUpdatedBy = dataSource.LastUpdatedBy
            };
        }

        /// <summary>
        /// Asynchronously updates an existing data source based on its ID.
        /// </summary>
        /// <param name="currentUserId">ID of the User executing this method.</param>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="dataSourceId">The ID of the existing data source to update.</param>
        /// <param name="dto">The data transfer object containing the updated data source details</param>
        /// <returns>The updated data source.</returns>
        /// <exception cref="KeyNotFoundException">Returned if data source not found</exception>
        public async Task<DataSourceResponseDto> UpdateDataSource(
            long currentUserId,
            long organizationId, 
            long projectId,
            long dataSourceId,
            UpdateDataSourceRequestDto dto)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);

            if (dataSource == null || dataSource.ProjectId != projectId || dataSource.IsArchived)
            {
                throw new KeyNotFoundException($"Data Source with id {dataSourceId} not found");
            }

            dataSource.Name = dto.Name ?? dataSource.Name;
            dataSource.Description = dto.Description ?? dataSource.Description;
            dataSource.Abbreviation = dto.Abbreviation ?? dataSource.Abbreviation;
            dataSource.BaseUri = dto.BaseUri ?? dataSource.BaseUri;
            dataSource.Config = dto.Config?.ToString() != null ? dto.Config.ToString() : new JsonObject().ToString();
            dataSource.Type = dto.Type ?? dataSource.Type;
            dataSource.LastUpdatedBy = currentUserId;
            dataSource.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            //_context.DataSources.Update(dataSource);
            await _context.SaveChangesAsync();
            
            await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
            {
                ProjectId = projectId,
                Operation = "update",
                EntityType = "data_source",
                EntityId = dataSource.Id,
                DataSourceId = null,
                EntityName = dataSource.Name,
                Properties = JsonSerializer.Serialize(new {dataSource.Name}),
            });

            return new DataSourceResponseDto
            {
                Id = dataSource.Id,
                Name = dataSource.Name,
                Description = dataSource.Description,
                Default = dataSource.Default,
                Abbreviation = dataSource.Abbreviation,
                Type = dataSource.Type,
                BaseUri = dataSource.BaseUri,
                // return empty object for config if null
                Config = JsonNode.Parse(dataSource.Config ?? "{}") as JsonObject,
                ProjectId = dataSource.ProjectId,
                LastUpdatedAt = dataSource.LastUpdatedAt,
                LastUpdatedBy = dataSource.LastUpdatedBy,
                IsArchived = dataSource.IsArchived,
            };
        }

        /// <summary>
        /// Deletes a specific data source by its ID.
        /// </summary>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <param name="dataSourceId">The ID of the data source to delete</param>
        /// <returns>Boolean true on successful deletion.</returns>
        /// <exception cref="KeyNotFoundException">Returned if data source not found or if ids missing</exception>
        public async Task<bool> DeleteDataSource(long organizationId, long projectId, long dataSourceId)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);

            if (dataSource == null || dataSource.ProjectId != projectId)
                throw new KeyNotFoundException($"Data Source with id {dataSourceId} not found");

            _context.DataSources.Remove(dataSource);
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Archives a specific data source by its ID.
        /// </summary>
        /// <param name="currentUserId">ID of the User executing this method.</param>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <param name="dataSourceId">The ID of the data source to archive</param>
        /// <returns>Boolean true on successful archival.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if data source is not found</exception>
        public async Task<bool> ArchiveDataSource(long currentUserId, long organizationId, long projectId, long dataSourceId)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);

            if (dataSource == null || dataSource.ProjectId != projectId || dataSource.IsArchived)
                throw new KeyNotFoundException($"Data Source with id {dataSourceId} not found");
            
            dataSource.IsArchived = true;
            dataSource.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            dataSource.LastUpdatedBy = currentUserId;
            
            await _context.SaveChangesAsync();
            
            // Log dataSource archive event
            await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
            {
                ProjectId = projectId,
                Operation = "archive",
                EntityType = "data_source",
                EntityId = dataSource.Id,
                DataSourceId = null,
                EntityName = dataSource.Name,
                Properties = JsonSerializer.Serialize(new {dataSource.Name}),
            });
            
            return true;
        }
        
        /// <summary>
        /// Unarchives a specific data source by its ID.
        /// </summary>
        /// <param name="currentUserId">ID of the User executing this method.</param>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <param name="dataSourceId">The ID of the data source to unarchive</param>
        /// <returns>Boolean true on successful unarchive action.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if data source is not found</exception>
        public async Task<bool> UnarchiveDataSource(long currentUserId, long organizationId, long projectId, long dataSourceId)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);

            if (dataSource == null || dataSource.ProjectId != projectId || !dataSource.IsArchived)
                throw new KeyNotFoundException($"Data Source with id {dataSourceId} not found or is not archived.");
            
            dataSource.IsArchived = false;
            dataSource.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            dataSource.LastUpdatedBy = currentUserId;
            _context.DataSources.Update(dataSource);
            await _context.SaveChangesAsync();
            
            // Log dataSource unarchive event
            await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
            {
                ProjectId = projectId,
                Operation = "unarchive",
                EntityType = "data_source",
                EntityId = dataSource.Id,
                EntityName = dataSource.Name,
                DataSourceId = null,
                Properties = JsonSerializer.Serialize(new {dataSource.Name}),
            });
            
            return true;
        }
        
        /// <summary>
        /// Sets an existing data source as default for a project.
        /// </summary>
        /// <param name="currentUserId">ID of the User executing this method.</param>
        /// <param name="organizationId">ID of the organization</param>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="dataSourceId">The ID of the existing data source to update.</param>
        /// <returns>The updated data source.</returns>
        /// <exception cref="KeyNotFoundException">Returned if data source not found</exception>
        public async Task<DataSourceResponseDto> SetDefaultDataSource(
            long currentUserId,
            long organizationId, 
            long projectId,
            long dataSourceId)
        {
            await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);

            if (dataSource == null || dataSource.ProjectId != projectId || dataSource.IsArchived)
            {
                throw new KeyNotFoundException($"Data Source with id {dataSourceId} not found");
            }

            if (!dataSource.Default)
            {
                dataSource.Default = true;
                dataSource.LastUpdatedBy = currentUserId;
                dataSource.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                await MakePreviousDefaultsFalse(currentUserId, currentUserId, projectId, dataSource.Id);
                await _context.SaveChangesAsync();

                await _eventBusiness.CreateEvent(currentUserId, new CreateEventRequestDto
                {
                    ProjectId = projectId,
                    Operation = "update",
                    EntityType = "data_source",
                    EntityId = dataSource.Id,
                    EntityName = dataSource.Name,
                    DataSourceId = null,
                    Properties = JsonSerializer.Serialize(new { dataSource.Name }),
                });
            }

            return new DataSourceResponseDto
            {
                Id = dataSource.Id,
                Name = dataSource.Name,
                Description = dataSource.Description,
                Default = dataSource.Default,
                Abbreviation = dataSource.Abbreviation,
                Type = dataSource.Type,
                BaseUri = dataSource.BaseUri,
                // return empty object for config if null
                Config = JsonNode.Parse(dataSource.Config ?? "{}") as JsonObject,
                ProjectId = dataSource.ProjectId,
                LastUpdatedAt = dataSource.LastUpdatedAt,
                LastUpdatedBy = dataSource.LastUpdatedBy,
                IsArchived = dataSource.IsArchived
            };
        }
        
        private async Task MakePreviousDefaultsFalse(long currentUserId, long organizaionId, long projectId, long defaultDataSourceId)
        {
            var previousDefaults = 
                await _context.DataSources
                    .Where(ds => ds.ProjectId == projectId && ds.Default == true && ds.Id != defaultDataSourceId)
                    .ToListAsync();
        
            if (previousDefaults.Count > 0)
            {
                foreach (var previousDefault in previousDefaults)
                {
                    previousDefault.Default = false;
                    previousDefault.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                    previousDefault.LastUpdatedBy = currentUserId;
                    _context.DataSources.Update(previousDefault);
                }
            }
        }
        
    }
}