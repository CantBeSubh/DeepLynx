using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface IDataSourceBusiness
    {
        Task<IEnumerable<DataSourceResponseDto>> GetAllDataSources(long projectId);
        Task<DataSourceResponseDto> GetDataSource(long projectId, long dataSourceId);
        Task<DataSourceResponseDto> CreateDataSource(long projectId, DataSourceRequestDto dto);
        Task<DataSourceResponseDto> UpdateDataSource(long projectId, long dataSourceId, DataSourceRequestDto dto);
        Task<bool> DeleteDataSource(long projectId, long dataSourceId, bool force);
        Task<bool> SoftDeleteAllDataSourcesByProjectIdAsync(long projectId);
    }
}