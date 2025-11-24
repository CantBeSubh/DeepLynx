using deeplynx.models;

namespace deeplynx.interfaces;

public interface IDataSourceBusiness
{
    Task<List<DataSourceResponseDto>>
        GetAllDataSources(long organizationId, long? projectId = null, bool hideArchived = true);

    Task<DataSourceResponseDto> GetDataSource(long organizationId, long dataSourceId,
        bool hideArchived = true, long? projectId = null);

    Task<DataSourceResponseDto> GetDefaultDataSource(long organizationId, long? projectId = null);

    Task<DataSourceResponseDto> SetDefaultDataSource(long currentUserId, long organizationId,
        long dataSourceId, long? projectId = null);

    Task<DataSourceResponseDto> CreateDataSource(long currentUserId, long organizationId,
        CreateDataSourceRequestDto dto, long? projectId = null);

    Task<DataSourceResponseDto> UpdateDataSource(long currentUserId, long organizationId,
        long dataSourceId, UpdateDataSourceRequestDto dto, long? projectId = null);

    Task<bool> DeleteDataSource(long organizationId, long dataSourceId, long? projectId = null);
    Task<bool> ArchiveDataSource(long currentUserId, long organizationId, long dataSourceId, long? projectId = null);
    Task<bool> UnarchiveDataSource(long currentUserId, long organizationId, long dataSourceId, long? projectId = null);
}