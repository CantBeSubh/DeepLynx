using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface IDataSourceBusiness
    {
        IEnumerable<DataSourceDto> GetAllDataSources();
        DataSourceDto CreateDataSource(DataSourceDto dataSourceDto);
        DataSourceDto UpdateDataSource(long id, DataSourceDto dataSourceDto);
        bool DeleteDataSource(long id);
    }
}