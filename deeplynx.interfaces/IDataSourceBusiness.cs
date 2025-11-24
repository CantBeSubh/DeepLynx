using deeplynx.models;

namespace deeplynx.interfaces;

public interface IDataSourceBusiness
{
    Task<List<DataSourceResponseDto>> GetAllDataSources(long organizationId, long? projectIds, bool hideArchived);

    Task<DataSourceResponseDto> GetDataSource(long organizationId, long? projectId, long dataSourceId,
        bool hideArchived);

    Task<DataSourceResponseDto> GetDefaultDataSource(long organizationId, long? projectId);

    Task<DataSourceResponseDto> SetDefaultDataSource(long currentUserId, long organizationId, long? projectId,
        long dataSourceId);

    Task<DataSourceResponseDto> CreateDataSource(long currentUserId, long organizationId, long? projectId,
        CreateDataSourceRequestDto dto);

    Task<DataSourceResponseDto> UpdateDataSource(long currentUserId, long organizationId, long? projectId,
        long dataSourceId,
        UpdateDataSourceRequestDto dto);

    Task<bool> DeleteDataSource(long organizationId, long? projectId, long dataSourceId);
    Task<bool> ArchiveDataSource(long currentUserId, long organizationId, long? projectId, long dataSourceId);
    Task<bool> UnarchiveDataSource(long currentUserId, long organizationId, long? projectId, long dataSourceId);
}