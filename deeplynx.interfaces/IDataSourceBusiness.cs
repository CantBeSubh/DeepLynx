using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;
using deeplynx.datalayer.Models;
using System.Linq.Expressions;

namespace deeplynx.interfaces
{
    public interface IDataSourceBusiness
    {
        Task<List<DataSourceResponseDto>> GetAllDataSources(long projectId, bool hideArchived);
        Task<DataSourceResponseDto> GetDataSource(long projectId, long dataSourceId, bool hideArchived);
        Task<DataSourceResponseDto> GetDefaultDataSource(long projectId);
        Task<DataSourceResponseDto> CreateDataSource(long currentUserId, long projectId, CreateDataSourceRequestDto dto, bool makeDefault = false);
        Task<DataSourceResponseDto> UpdateDataSource(long currentUserId, long projectId, long dataSourceId, UpdateDataSourceRequestDto dto);
        Task<DataSourceResponseDto> SetDefaultDataSource(long currentUserId, long projectId, long dataSourceId);
        Task<bool> DeleteDataSource(long projectId, long dataSourceId);
        Task<bool> ArchiveDataSource(long currentUserId, long projectId, long dataSourceId);
        Task<bool> UnarchiveDataSource(long currentUserId, long projectId, long dataSourceId);
    }
}