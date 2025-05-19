using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;


namespace deeplynx.business
{

    public class DataSourceBusiness : IDataSourceBusiness
    {
        private readonly DeeplynxContext _context;

        public DataSourceBusiness(DeeplynxContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Get all data sources that exist and map to dto
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<DataSourceDto>> GetAllDataSources()
        {
            return await _context.DataSources.Select(ds => new DataSourceDto
            {
                Id = ds.Id,
                Name = ds.Name,
                ProjectId = ds.ProjectId
            }).ToListAsync();
        }

        /// <summary>
        /// Create new data source and return dto 
        /// </summary>
        /// <param name="dataSourceDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<DataSourceDto> CreateDataSource(DataSourceDto dataSourceDto)
        {
            if (dataSourceDto == null)
                throw new ArgumentNullException(nameof(dataSourceDto));

            var dataLayerDataSource = new DataSource
            {
                Name = dataSourceDto.Name,
                ProjectId = dataSourceDto.ProjectId,
                Abbreviation = dataSourceDto.Abbreviation,
                Config = dataSourceDto.Config,
                Type = dataSourceDto.Type,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ModifiedBy = null
            };

            _context.DataSources.Add(dataLayerDataSource);
            await _context.SaveChangesAsync();

            dataSourceDto.Id = dataLayerDataSource.Id;
            return dataSourceDto;
        }

        /// <summary>
        /// Update a current data source and return dto 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataSourceDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<DataSourceDto> UpdateDataSource(long id, DataSourceDto dataSourceDto)
        {
            var existing = await _context.DataSources.FirstOrDefaultAsync(ds => ds.Id == id);
            if (existing == null)
                throw new Exception("DataSource not found");

            existing.Name = dataSourceDto.Name;
            existing.Abbreviation = dataSourceDto.Abbreviation;
            existing.BaseUri = dataSourceDto.BaseUri;
            existing.Config = dataSourceDto.Config;
            existing.Type = dataSourceDto.Type;
            existing.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();
            return dataSourceDto;
        }

        /// <summary>
        /// Soft delete data source using UTC field Deleted_At
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteDataSource(long id)
        {
            var existing = await _context.DataSources.FirstOrDefaultAsync(ds => ds.Id == id);
            if (existing == null)
                return false; 
            existing.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);;
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