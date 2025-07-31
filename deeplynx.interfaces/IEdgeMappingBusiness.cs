using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IEdgeMappingBusiness
{
    Task<IEnumerable<EdgeMappingResponseDto>> GetAllEdgeMappings(long projectId, long? classId, long? relationshipId, bool hideArchived);
    Task<EdgeMappingResponseDto> GetEdgeMapping(long projectId, long mappingId, bool hideArchived);
    Task<EdgeMappingResponseDto> CreateEdgeMapping(long projectId, EdgeMappingRequestDto dto);
    Task<EdgeMappingResponseDto> UpdateEdgeMapping(long projectId, long mappingId, UpdateEdgeMappingRequestDto dto);
    Task<bool> DeleteEdgeMapping(long projectId, long mappingId);
    Task<bool> ArchiveEdgeMapping(long projectId, long mappingId);
    Task<bool> UnarchiveEdgeMapping(long projectId, long mappingId);
}