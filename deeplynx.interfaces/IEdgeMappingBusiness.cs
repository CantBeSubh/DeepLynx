using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IEdgeMappingBusiness
{
    Task<IEnumerable<EdgeMapping>> GetAllEdgeMappings(long projectId);
    Task<EdgeMapping> GetEdgeMapping(long mappingId);
    Task<EdgeMapping> CreateEdgeMapping(long projectId, EdgeMappingRequestDto dto);
    Task<EdgeMapping> UpdateEdgeMapping(long projectId, long mappingId, EdgeMappingRequestDto dto);
    Task<bool> DeleteEdgeMapping(long mappingId);
}