using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IEdgeBusiness
{
    Task<List<EdgeResponseDto>> GetAllEdges(
        long projectId, long? dataSourceId, bool hideArchived);
    Task<List<RelatedRecordsResponseDto>> GetEdgesByRecord(long recordId, bool hideArchived);
    Task<EdgeResponseDto> GetEdge(
        long projectId, long? edgeId, long? originId, long? destinationId, bool hideArchived);
    Task<EdgeResponseDto> CreateEdge(
        long projectId, long dataSourceId, CreateEdgeRequestDto edge);
    Task<List<EdgeResponseDto>> BulkCreateEdges(
        long projectId, long dataSourceId, List<CreateEdgeRequestDto> edgeRequestDtos);
    Task<EdgeResponseDto> UpdateEdge(
        long projectId, UpdateEdgeRequestDto edge, long? edgeId, long? originId, long? destinationId);
    Task<long> DeleteEdge(
        long projectId, long? edgeId, long? originId, long? destinationId);
    Task<long> ArchiveEdge(
        long projectId, long? edgeId, long? originId, long? destinationId);
    Task<long> UnarchiveEdge(
        long projectId, long? edgeId, long? originId, long? destinationId);
}