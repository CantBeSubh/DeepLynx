using System.Data;
using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business
{
    public class DataSourceBusiness : IDataSourceBusiness
    {
        private readonly DeeplynxContext _context;

        // dependants used to trigger downstream soft deletes
        private readonly IEdgeBusiness _edgeBusiness;
        private readonly IRecordBusiness _recordBusiness;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceBusiness"/> class.
        /// </summary>
        /// <param name="context">The database context used for the data source operations.</param>
        /// <param name="edgeBusiness">Passed in context for downstream edge objects.</param>
        /// <param name="recordBusiness">Passed in context for downstream record objects.</param>
        public DataSourceBusiness(
            DeeplynxContext context,
            IEdgeBusiness edgeBusiness,
            IRecordBusiness recordBusiness)
        {
            _context = context;
            _edgeBusiness = edgeBusiness;
            _recordBusiness = recordBusiness;
        }

        /// <summary>
        /// Retrieves all data sources for a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project whose data sources are to be retrieved</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result</param>
        /// <returns>A list of data sources within the given project.</returns>
        public async Task<IEnumerable<DataSourceResponseDto>> GetAllDataSources(long projectId, bool hideArchived)
        {
            DoesProjectExist(projectId, hideArchived);
            var dataSources = await _context.DataSources
                .Where(d => d.ProjectId == projectId).ToListAsync();

            if (hideArchived)
            {
                dataSources = dataSources.Where(d => d.ArchivedAt == null).ToList();
            }
            
            return dataSources
                .Select(d => new DataSourceResponseDto()
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    Abbreviation = d.Abbreviation,
                    Type = d.Type,
                    BaseUri = d.BaseUri,
                    // return empty object for config if null
                    Config = JsonNode.Parse(d.Config ?? "{}") as JsonObject,
                    ProjectId = d.ProjectId,
                    CreatedBy = d.CreatedBy,
                    CreatedAt = d.CreatedAt,
                    ModifiedBy = d.ModifiedBy,
                    ModifiedAt = d.ModifiedAt,
                    ArchivedAt = d.ArchivedAt,
                });
        }

        /// <summary>
        /// Retrieve a specific data source by its ID
        /// </summary>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="datasourceId">The ID of the data source</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived data sources from the result</param>
        /// <returns>The data source in question</returns>
        /// <exception cref="KeyNotFoundException">Returned if the data source is not found or is archived</exception>
        public async Task<DataSourceResponseDto> GetDataSource(long projectId, long datasourceId, bool hideArchived)
        {
            DoesProjectExist(projectId, hideArchived);
            var dataSource = await _context.DataSources
                .Where(d => d.ProjectId == projectId && d.Id == datasourceId)
                .FirstOrDefaultAsync();

            if (dataSource == null || dataSource.ProjectId != projectId)
            {
                throw new KeyNotFoundException($"Data Source with id {datasourceId} not found");
            }
            
            if (hideArchived && dataSource.ArchivedAt != null)
            {
                throw new KeyNotFoundException($"Data Source with id {datasourceId} is archived");
            }

            return new DataSourceResponseDto
            {
                Id = dataSource.Id,
                Name = dataSource.Name,
                Description = dataSource.Description,
                Abbreviation = dataSource.Abbreviation,
                Type = dataSource.Type,
                BaseUri = dataSource.BaseUri,
                // return empty object for config if null
                Config = JsonNode.Parse(dataSource.Config ?? "{}") as JsonObject,
                ProjectId = dataSource.ProjectId,
                CreatedBy = dataSource.CreatedBy,
                CreatedAt = dataSource.CreatedAt,
                ModifiedBy = dataSource.ModifiedBy,
                ModifiedAt = dataSource.ModifiedAt,
                ArchivedAt = dataSource.ArchivedAt,
            };
        }

        /// <summary>
        /// Asynchronously creates a new data source for a specified project.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="dto">The data transfer object containing data source details</param>
        /// <returns>The created data source.</returns>
        public async Task<DataSourceResponseDto> CreateDataSource(long projectId, DataSourceRequestDto dto)
        {
            DoesProjectExist(projectId);
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var dataSource = new DataSource
            {
                Name = dto.Name,
                ProjectId = projectId,
                Description = dto.Description,
                BaseUri = dto.BaseUri,
                Abbreviation = dto.Abbreviation,
                Config = dto.Config?.ToString(),
                Type = dto.Type,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
            };

            await _context.DataSources.AddAsync(dataSource);
            await _context.SaveChangesAsync();

            return new DataSourceResponseDto
            {
                Id = dataSource.Id,
                Name = dataSource.Name,
                Description = dataSource.Description,
                Abbreviation = dataSource.Abbreviation,
                Type = dataSource.Type,
                BaseUri = dataSource.BaseUri,
                // return empty object for config if null
                Config = JsonNode.Parse(dataSource.Config ?? "{}") as JsonObject,
                ProjectId = dataSource.ProjectId,
                CreatedBy = dataSource.CreatedBy,
                CreatedAt = dataSource.CreatedAt
            };
        }

        /// <summary>
        /// Asynchronously updates an existing data source based on its ID.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="dataSourceId">The ID of the existing data source to update.</param>
        /// <param name="dto">The data transfer object containing the updated data source details</param>
        /// <returns>The updated data source.</returns>
        /// <exception cref="KeyNotFoundException">Returned if data source not found</exception>
        public async Task<DataSourceResponseDto> UpdateDataSource(
            long projectId,
            long dataSourceId,
            DataSourceRequestDto dto)
        {
            DoesProjectExist(projectId);
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);

            if (dataSource == null || dataSource.ProjectId != projectId || dataSource.ArchivedAt is not null)
            {
                throw new KeyNotFoundException($"Data Source with id {dataSourceId} not found");
            }

            dataSource.Name = dto.Name;
            dataSource.Description = dto.Description;
            dataSource.Abbreviation = dto.Abbreviation;
            dataSource.BaseUri = dto.BaseUri;
            dataSource.Config = dto.Config?.ToString();
            dataSource.Type = dto.Type;
            dataSource.ModifiedBy = null; // TODO: handled in future by JWT.
            dataSource.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            //_context.DataSources.Update(dataSource);
            await _context.SaveChangesAsync();

            return new DataSourceResponseDto
            {
                Id = dataSource.Id,
                Name = dataSource.Name,
                Description = dataSource.Description,
                Abbreviation = dataSource.Abbreviation,
                Type = dataSource.Type,
                BaseUri = dataSource.BaseUri,
                // return empty object for config if null
                Config = JsonNode.Parse(dataSource.Config ?? "{}") as JsonObject,
                ProjectId = dataSource.ProjectId,
                CreatedBy = dataSource.CreatedBy,
                CreatedAt = dataSource.CreatedAt,
                ModifiedBy = dataSource.ModifiedBy,
                ModifiedAt = dataSource.ModifiedAt
            };
        }

        /// <summary>
        /// Deletes a specific data source by its ID.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <param name="dataSourceId">The ID of the data source to delete</param>
        /// <returns>Boolean true on successful deletion.</returns>
        /// <exception cref="KeyNotFoundException">Returned if data source not found or if ids missing</exception>
        public async Task<bool> DeleteDataSource(long projectId, long dataSourceId)
        {
            DoesProjectExist(projectId);
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
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <param name="dataSourceId">The ID of the data source to archive</param>
        /// <returns>Boolean true on successful archival.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if data source is not found</exception>
        public async Task<bool> ArchiveDataSource(long projectId, long dataSourceId)
        {
            DoesProjectExist(projectId);
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);

            if (dataSource == null || dataSource.ProjectId != projectId || dataSource.ArchivedAt is not null)
                throw new KeyNotFoundException($"Data Source with id {dataSourceId} not found");
            // set archivedAt timestamp
            var archivedAt = DateTime.UtcNow;
            
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // run the archive data source procedure, which archives this datasource
                    // and all child objects with data_source_id as a foreign key
                    var archived = await _context.Database.ExecuteSqlRawAsync(
                        "CALL deeplynx.archive_data_source({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)", dataSourceId,
                        archivedAt);

                    if (archived == 0) // if 0 records were updated, assume a failure
                    {
                        throw new DependencyDeletionException(
                            $"unable to archive data source {dataSourceId} or its downstream dependents.");
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception exc)
                {
                    await transaction.RollbackAsync();
                    throw new DependencyDeletionException(
                        $"unable to archive data source {dataSourceId} or its downstream dependents: {exc}");
                }
            }
        }
        
        /// <summary>
        /// Unarchives a specific data source by its ID.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <param name="dataSourceId">The ID of the data source to unarchive</param>
        /// <returns>Boolean true on successful unarchive action.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if data source is not found</exception>
        public async Task<bool> UnarchiveDataSource(long projectId, long dataSourceId)
        {
            DoesProjectExist(projectId);
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);

            if (dataSource == null || dataSource.ProjectId != projectId || dataSource.ArchivedAt is null)
                throw new KeyNotFoundException($"Data Source with id {dataSourceId} not found or is not archived.");
            
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // run the unarchive data source procedure, which unarchives this datasource
                    // and all child objects with data_source_id as a foreign key
                    var unarchived = await _context.Database.ExecuteSqlRawAsync(
                        "CALL deeplynx.unarchive_data_source({0}::INTEGER)", dataSourceId);

                    if (unarchived == 0) // if 0 records were updated, assume a failure
                    {
                        throw new DependencyDeletionException(
                            $"unable to unarchive data source {dataSourceId} or its downstream dependents.");
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception exc)
                {
                    await transaction.RollbackAsync();
                    throw new DependencyDeletionException(
                        $"unable to unarchive data source {dataSourceId} or its downstream dependents: {exc}");
                }
            }
        }
        
        /// <summary>
        /// Determine if project exists
        /// </summary>
        /// <param name="projectId">The ID of the project we are searching for</param>
        /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
        /// <returns>Throws error if project does not exist</returns>
        private void DoesProjectExist(long projectId, bool hideArchived = true)
        {
            var project = hideArchived ? _context.Projects.Any(p => p.Id == projectId && p.ArchivedAt == null) 
                : _context.Projects.Any(p => p.Id == projectId);
            if (!project)
            {
                throw new KeyNotFoundException($"Project with id {projectId} not found");
            }
        }
    }
}