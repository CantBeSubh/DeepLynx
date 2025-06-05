using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business
{
    public class DataSourceBusiness : IDataSourceBusiness
    {
        private readonly DeeplynxContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceBusiness"/> class.
        /// </summary>
        /// <param name="context">The database context used for the data source operations.</param>
        public DataSourceBusiness(DeeplynxContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Retrieves all data sources for a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project whose data sources are to be retrieved</param>
        /// <returns>A list of data sources within the given project.</returns>
        public async Task<IEnumerable<DataSourceResponseDto>> GetAllDataSources(long projectId)
        {
            var dataSources = await _context.DataSources
                .Where(d => d.ProjectId == projectId && d.DeletedAt == null).ToListAsync();

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
                    ModifiedAt = d.ModifiedAt
                });
        }

        /// <summary>
        /// Retrieve a specific data source by its ID
        /// </summary>
        /// <param name="projectId">The ID of the project to which the data source belongs</param>
        /// <param name="datasourceId">The ID of the data source</param>
        /// <returns>The data source in question</returns>
        /// <exception cref="KeyNotFoundException">Returned if the data source is not found</exception>
        public async Task<DataSourceResponseDto> GetDataSource(long projectId, long datasourceId)
        {
            var dataSource = await _context.DataSources
                .Where(d => d.ProjectId == projectId && d.Id == datasourceId && d.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (dataSource == null || dataSource.ProjectId != projectId || dataSource.DeletedAt is not null)
            {
                throw new KeyNotFoundException($"Data Source with id {datasourceId} not found");
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
                ModifiedAt = dataSource.ModifiedAt
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

            _context.DataSources.Add(dataSource);
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
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);
            
            if (dataSource == null || dataSource.ProjectId != projectId || dataSource.DeletedAt is not null)
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

            _context.DataSources.Update(dataSource);
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
        /// Deletes a specific data source by its ID or origin/destination.
        /// </summary>
        /// <param name="projectId">The ID of the project to which the data source belongs.</param>
        /// <param name="dataSourceId">The ID of the data source to delete</param>
        /// <param name="force">Indicates whether to force delete the data source if true.</param>
        /// <returns>Boolean true on successful deletion.</returns>
        /// <exception cref="KeyNotFoundException">Returned if data source not found or if ids missing</exception>
        public async Task<bool> DeleteDataSource(long projectId, long dataSourceId, bool force = false)
        {
            var dataSource = await _context.DataSources.FindAsync(dataSourceId);
            
            if (dataSource == null || dataSource.ProjectId != projectId || dataSource.DeletedAt is not null)
            {
                throw new KeyNotFoundException($"Data Source with id {dataSourceId} not found");
            }

            if (force)
            {
                _context.DataSources.Remove(dataSource);
            }
            else
            {
                // soft delete
                dataSource.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                _context.DataSources.Update(dataSource);
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        /// <summary>
        /// Called primarily by project's delete. Soft delete all data sources in a project by project id.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>Boolean true on successful deletion.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<bool> SoftDeleteAllDataSourcesByProjectIdAsync(long projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found.");
        
            try
            {
                var dataSources = await _context.DataSources.Where(t => t.ProjectId == projectId && t.DeletedAt == null).ToListAsync();
                foreach (var dataSource in dataSources)
                {
                    dataSource.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception exception)
            {
                var message = $"An error occurred while deleting project data sources: {exception}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return false;
            }
        }
    }
}