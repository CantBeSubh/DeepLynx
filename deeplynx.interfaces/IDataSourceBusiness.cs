using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface IDataSourceBusiness
    {
        IEnumerable<DataSource> GetDataSources();
        DataSource CreateDataSource(DataSource dataSource);
        DataSource UpdateDataSource(long id, DataSource dataSource);
        bool DeleteDataSource(long id);
    }
}