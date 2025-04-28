using deeplynx.interface;
using deeplynx.datalayer.database;

namespace deeplynx.business
{
    using DataLayerDataSource = datalayer.Models.DataSource;
    using BusinessLayerDataSource = models.DataSource;

    public class DataSourceBusiness : IDataSourceBusiness
    {
        private readonly DeepLynxDatabaseContext _context;

        public DataSourceBusiness(DeepLynxDatabaseContext context)
        {
            _context = context;
        }

        public IEnumerable<BusinessLayerDataSource> GetDataSources()
        {
            return _context.DataSources.Select(ds => new BusinessLayerDataSource
            {
                Id = ds.Id,
                Name = ds.Name,
                ProjectId = ds.ProjectId
            }).ToList();
        }

        public BusinessLayerDataSource CreateDataSource(BusinessLayerDataSource dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            var dataLayerDataSource = new DataLayerDataSource
            {
                Name = dataSource.Name,
                ProjectId = dataSource.ProjectId
            };

            _context.DataSources.Add(dataLayerDataSource);
            _context.SaveChanges();

            dataSource.Id = dataLayerDataSource.Id;
            return dataSource;
        }

        public BusinessLayerDataSource UpdateDataSource(long id, BusinessLayerDataSource dataSource)
        {
            var existing = _context.DataSources.FirstOrDefault(ds => ds.Id == id);
            if (existing == null)
                throw new Exception("DataSource not found");

            existing.Name = dataSource.Name;

            _context.SaveChanges();
            return dataSource;
        }

        public bool DeleteDataSource(long id)
        {
            var existing = _context.DataSources.FirstOrDefault(ds => ds.Id == id);
            if (existing == null)
                return false; 

            _context.DataSources.Remove(existing);
            _context.SaveChanges();
            return true;
        }
    }
}