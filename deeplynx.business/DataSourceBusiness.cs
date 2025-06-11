using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

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
                // hard delete
                _context.DataSources.Remove(dataSource);
                await _context.SaveChangesAsync();
            }
            else
            {
                try
                {
                    var transaction = await _context.Database.BeginTransactionAsync();
                    await this.SoftDeleteDataSources(d => d.Id == dataSourceId, transaction);
                    await transaction.CommitAsync();
                }
                catch (Exception exc)
                {
                    var message = $"An error occurred while deleting data source: {exc}";
                    NLog.LogManager.GetCurrentClassLogger().Error(message);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Bulk Soft Delete data sources by a specific upstream domain. Used to avoid repeating functions.
        /// </summary>
        /// <param name="predicate">an anonymous function that allows the context to be filtered appropriately</param>
        /// <param name="transaction">(Optional) a transaction passed in from the parent to ensure ACID compliance</param>
        /// <returns>Boolean true on successful deletion</returns>
        public async Task<bool> BulkSoftDeleteDataSources(
            Expression<Func<DataSource, bool>> predicate,
            IDbContextTransaction? transaction)
        {
            try
            {
                await this.SoftDeleteDataSources(predicate, transaction);
                return true;
            }
            catch (Exception exc)
            {
                var message = $"An error occurred while deleting data sources: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return false;
            }
        }

        private async Task SoftDeleteDataSources(
            Expression<Func<DataSource, bool>> predicate,
            IDbContextTransaction? transaction)
        {
            // check for existing transaction; if one does not exist, start a new one
            var commit = false; // flag used to determine if transaction should be committed
            if (transaction == null)
            {
                commit = true;
                transaction = await _context.Database.BeginTransactionAsync();
            }

            // search for records matching the passed-in predicate (filter) to be updated
            var dsContext = _context.DataSources
                .Where(d => d.DeletedAt == null)
                .Where(predicate);

            var dataSources = await dsContext.ToListAsync();
            
            if (dataSources.Count == 0)
            {
                // return true even if no records are to be deleted;
                // we only want to return false if there were errors
                return;
            }
            
            var dataSourceIds = dataSources.Select(d => d.Id);
            
            // trigger downstream deletions
            var softDeleteTasks = new List<Func<Task<bool>>>
            {
                () => _recordBusiness.BulkSoftDeleteRecords("dataSource", dataSourceIds, transaction),
                () => _edgeBusiness.BulkSoftDeleteEdges("dataSource", dataSourceIds)
            };

            // loop through tasks and trigger downstream deletions
            foreach (var task in softDeleteTasks)
            {
                bool result = await task();
                if (!result)
                {
                    // rollback the transaction and throw an error
                    await transaction.RollbackAsync();
                    throw new ProjectDependencyDeletionException(
                        "An error occurred during the deletion of downstream datasource dependants.");
                }
            }

            // bulk update the results of the query to set the deleted_at date
            var updated = await dsContext.ExecuteUpdateAsync(setters => setters
                .SetProperty(ds => ds.DeletedAt, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)));

            // if we found records to update, but weren't successful in updating, throw an error
            if (updated == 0)
            {
                throw new ProjectDependencyDeletionException("An error occurred when deleting data sources");
            }

            // save changes and commit transaction to close it
            await _context.SaveChangesAsync();
            if (commit)
            {
                await transaction.CommitAsync();
            }
        }
    }
}