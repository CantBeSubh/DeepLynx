using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;


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
        public IEnumerable<DataSourceDto> GetAllDataSources()
        {
            return _context.DataSources.Select(ds => new DataSourceDto
            {
                Id = ds.Id,
                Name = ds.Name,
                ProjectId = ds.ProjectId
            }).ToList();
        }

        /// <summary>
        /// Create new data source and return dto 
        /// </summary>
        /// <param name="dataSourceDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public DataSourceDto CreateDataSource(DataSourceDto dataSourceDto)
        {
            if (dataSourceDto == null)
                throw new ArgumentNullException(nameof(dataSourceDto));

            var dataLayerDataSource = new DataSource
            {
                Name = dataSourceDto.Name,
                ProjectId = dataSourceDto.ProjectId
            };

            _context.DataSources.Add(dataLayerDataSource);
            _context.SaveChanges();

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
        public DataSourceDto UpdateDataSource(long id, DataSourceDto dataSourceDto)
        {
            var existing = _context.DataSources.FirstOrDefault(ds => ds.Id == id);
            if (existing == null)
                throw new Exception("DataSource not found");

            existing.Name = dataSourceDto.Name;

            _context.SaveChanges();
            return dataSourceDto;
        }

        /// <summary>
        /// Soft delete data source using UTC field Deleted_At
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteDataSource(long id)
        {
            var existing = _context.DataSources.FirstOrDefault(ds => ds.Id == id);
            if (existing == null)
                return false; 
            existing.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);;
            _context.SaveChanges();
            return true;
        }
    }
}