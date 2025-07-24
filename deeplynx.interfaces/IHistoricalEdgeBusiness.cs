using System.Collections;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IHistoricalEdgeBusiness
{
    Task<IEnumerable<HistoricalEdgeResponseDto>> GetAllHistoricalEdges(
        long projectId, long? dataSourceId, DateTime? pointInTime, 
        bool hideArchived, bool current);
    Task<IEnumerable<HistoricalEdgeResponseDto>> GetHistoryForEdge(
        long? edgeId, long? originId, long? destinationId);
    Task<HistoricalEdgeResponseDto> GetHistoricalEdge(
        long? edgeId, long? originId, long? destinationId,
        DateTime? pointInTime, bool hideArchived, bool current);
}