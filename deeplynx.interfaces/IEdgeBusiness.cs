using deeplynx.models;

namespace deeplynx.interfaces;

public interface IEdgeBusiness
{
    Task<List<EdgeResponseDto>> GetAllEdges(
        long projectId, long? dataSourceId, bool hideArchived);

    Task<EdgeResponseDto> GetEdge(
        long projectId, long organizationId, long? edgeId, long? originId, long? destinationId, bool hideArchived);

    Task<EdgeResponseDto> CreateEdge(
        long currentUserId, long projectId, long dataSourceId, long organizationId, CreateEdgeRequestDto edge);

    Task<List<EdgeResponseDto>> BulkCreateEdges(
        long currentUserId, long projectId, long dataSourceId, long organizationId,
        List<CreateEdgeRequestDto> edgeRequestDtos);

    Task<EdgeResponseDto> UpdateEdge(
        long currentUserId, long projectId, long organizationId, UpdateEdgeRequestDto edge, long? edgeId,
        long? originId,
        long? destinationId);

    Task<long> DeleteEdge(
        long projectId, long organizationId, long? edgeId, long? originId, long? destinationId);

    Task<long> ArchiveEdge(
        long currentUserId, long organizationId, long projectId, long? edgeId, long? originId, long? destinationId);

    Task<long> UnarchiveEdge(
        long currentUserId, long projectId, long organizationId, long? edgeId, long? originId, long? destinationId);
}