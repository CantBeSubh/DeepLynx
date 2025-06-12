using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IEdgeBusiness
{
    Task<IEnumerable<EdgeResponseDto>> GetAllEdges(long projectId, long? dataSourceId);
    Task<EdgeResponseDto> GetEdge(long? edgeId, long? originId, long? destinationId);
    Task<EdgeResponseDto> CreateEdge(long projectId, long dataSourceId, EdgeRequestDto edge);
    Task<EdgeResponseDto> UpdateEdge(long projectId, EdgeRequestDto edge, long? edgeId, long? originId, long? destinationId);
    Task<bool> DeleteEdge(long projectId, long? edgeId, long? originId, long? destinationId, bool force=false);
    Task<bool> BulkSoftDeleteEdges(Expression<Func<Edge, bool>> predicate);
}