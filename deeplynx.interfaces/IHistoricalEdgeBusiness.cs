using deeplynx.models;

namespace deeplynx.interfaces;

public interface IHistoricalEdgeBusiness
{
    Task<IEnumerable<HistoricalEdgeResponseDto>> GetAllHistoricalEdges(
        long projectId, long? dataSourceId, DateTime? pointInTime,
        bool hideArchived);

    Task<IEnumerable<HistoricalEdgeResponseDto>> GetHistoryForEdge(
        long organizationId, long? edgeId, long? originId, long? destinationId);

    Task<HistoricalEdgeResponseDto> GetHistoricalEdge(
        long organizationId, long? edgeId, long? originId, long? destinationId,
        DateTime? pointInTime, bool hideArchived);
}