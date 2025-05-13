using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface IDataSourceBusiness
    {
        Task<IEnumerable<DataSourceDto>> GetAllDataSources();
        Task<DataSourceDto> CreateDataSource(DataSourceDto dataSourceDto);
        Task<DataSourceDto> UpdateDataSource(long id, DataSourceDto dataSourceDto);
        Task<bool> DeleteDataSource(long id);
    }
}