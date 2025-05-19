using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IEdgeBusiness
{
    Task<IEnumerable<Edge>> GetAllEdges(long projectId, long? dataSourceId);
    Task<Edge> GetEdge(long originId, long destinationId);
    Task<Edge> CreateEdge(long projectId, long dataSourceId, EdgeRequestDto edge);
    Task<Edge> UpdateEdge(long originId, long destinationId, EdgeRequestDto edge);
    Task<bool> DeleteEdge(long originId, long destinationId);
    Task<bool> SoftDeleteAllEdgesByProjectIdAsync(long projectId);
}