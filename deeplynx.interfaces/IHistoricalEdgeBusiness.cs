using System.Collections;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IHistoricalEdgeBusiness
{
    Task<IEnumerable<HistoricalEdgeResponseDto>> GetAllHistoricalEdges(
        long projectId, 
        long? dataSourceId, 
        DateTime? startDate, 
        DateTime? endDate);
    Task<IEnumerable<HistoricalEdgeResponseDto>> GetHistoryForEdge(
        long edgeId,
        DateTime? startDate,
        DateTime? endDate);
    Task<long> CreateHistoricalEdge(long projectId, long dataSourceId, long edgeId);
    Task<long> UpdateHistoricalEdge(long projectId, long edgeId);
    Task<long> DeleteHistoricalEdge(long projectId, long edgeId);
    Task<long> ArchiveHistoricalEdge(long projectId, long edgeId);
}