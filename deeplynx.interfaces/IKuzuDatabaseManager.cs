using System.Collections.Generic;
using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface IKuzuDatabaseManager
    {
        Task<bool> ConnectAsync();
        Task CloseAsync();
        Task InstallPostgresExtensionsAsync();
        Task<bool> ExportDataAsync(int project_id);
        Task<bool> LoadDataAsync(int project_id);
        Task<string> ExecuteQueryAsync(KuzuDBMQueryRequestDto request);
        Task<string> GetNodesWithinDepthByIdAsync(KuzuDBMNodesWithinDepthRequestDto request);
    }
}