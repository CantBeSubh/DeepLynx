using System.Collections.Generic;
using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface IKuzuDatabaseManager
    {
        Task<bool> ConnectAsync();
        Task<bool> CloseAsync();
        Task<bool> ExportDataAsync(int project_id);
        Task<bool> LoadDataAsync(int project_id);
        Task<(string formattedString, object[] results)> ExecuteQueryAsync(KuzuDBMQueryRequestDto request, bool DoAddTenantIdToQuery=true);
        Task<(object[]? results, string formattedString)> GetNodesWithinDepthByIdAsync(KuzuDBMNodesWithinDepthRequestDto request);
    }
}