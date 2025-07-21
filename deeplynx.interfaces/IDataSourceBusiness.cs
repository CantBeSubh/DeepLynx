using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;
using deeplynx.datalayer.Models;
using System.Linq.Expressions;

namespace deeplynx.interfaces
{
    public interface IDataSourceBusiness
    {
        Task<IEnumerable<DataSourceResponseDto>> GetAllDataSources(long projectId, bool hideArchived);
        Task<DataSourceResponseDto> GetDataSource(long projectId, long dataSourceId, bool hideArchived);
        Task<DataSourceResponseDto> CreateDataSource(long projectId, DataSourceRequestDto dto);
        Task<DataSourceResponseDto> UpdateDataSource(long projectId, long dataSourceId, DataSourceRequestDto dto);
        Task<bool> DeleteDataSource(long projectId, long dataSourceId);
        Task<bool> ArchiveDataSource(long projectId, long dataSourceId);
        Task<bool> UnarchiveDataSource(long projectId, long dataSourceId);
    }
}