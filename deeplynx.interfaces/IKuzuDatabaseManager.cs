using System.Collections.Generic;
using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface IKuzuDatabaseManager
    {
        Task ConnectAsync();
        Task CloseAsync();
        Task<bool> ExportDataAsync(string pgParams, int? project_id);
        Task<bool> LoadDataAsync(int? project_id);
        Task<string> ExecuteQueryAsync(KuzuDatabaseManagerQueryRequestDto request);
    }
}