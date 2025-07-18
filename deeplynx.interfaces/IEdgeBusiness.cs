using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IEdgeBusiness
{
    Task<IEnumerable<HistoricalEdgeResponseDto>> GetAllEdges(
        long projectId, long? dataSourceId, bool hideArchived);
    Task<HistoricalEdgeResponseDto> GetEdge(
        long projectId, long? edgeId, long? originId, long? destinationId, bool hideArchived);
    Task<EdgeResponseDto> CreateEdge(
        long projectId, long dataSourceId, EdgeRequestDto edge);
    Task<BulkEdgeResponseDto> BulkCreateEdges(
        long projectId, long dataSourceId, BulkEdgeRequestDto bulkDto);
    Task<EdgeResponseDto> UpdateEdge(
        long projectId, EdgeRequestDto edge, long? edgeId, long? originId, long? destinationId);
    Task<long> DeleteEdge(
        long projectId, long? edgeId, long? originId, long? destinationId);
    Task<long> ArchiveEdge(
        long projectId, long? edgeId, long? originId, long? destinationId);
}