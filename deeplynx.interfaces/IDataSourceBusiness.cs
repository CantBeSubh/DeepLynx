using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;
using deeplynx.datalayer.Models;
using System.Linq.Expressions;

namespace deeplynx.interfaces
{
    public interface IDataSourceBusiness
    {
        Task<List<DataSourceResponseDto>> GetAllDataSources(long organizationId, long projectId, bool hideArchived);
        Task<DataSourceResponseDto> GetDataSource(long organizationId, long projectId, long dataSourceId, bool hideArchived);
        Task<DataSourceResponseDto> GetDefaultDataSource(long organizationId, long projectId);
        Task<DataSourceResponseDto> CreateDataSource(long organizationId, long currentUserId, long projectId, CreateDataSourceRequestDto dto, bool makeDefault = false);
        Task<DataSourceResponseDto> UpdateDataSource(long organizationId, long currentUserId, long projectId, long dataSourceId, UpdateDataSourceRequestDto dto);
        Task<DataSourceResponseDto> SetDefaultDataSource(long organizationId, long currentUserId, long projectId, long dataSourceId);
        Task<bool> DeleteDataSource(long organizationId, long projectId, long dataSourceId);
        Task<bool> ArchiveDataSource(long organizationId, long currentUserId, long projectId, long dataSourceId);
        Task<bool> UnarchiveDataSource(long organizationId, long currentUserId, long projectId, long dataSourceId);
    }
}