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
    Task<long> DeleteEdge(long projectId, long? edgeId, long? originId, long? destinationId);
    Task<long> ArchiveEdge(long projectId, long? edgeId, long? originId, long? destinationId);
}