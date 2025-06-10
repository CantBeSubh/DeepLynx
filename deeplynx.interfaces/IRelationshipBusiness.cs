using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRelationshipBusiness
{
    Task<IEnumerable<RelationshipResponseDto>> GetAllRelationships(long projectId);
    Task<RelationshipResponseDto> GetRelationship(long projectId, long relationshipId);
    Task<RelationshipResponseDto> CreateRelationship(long projectId, RelationshipRequestDto dto);
    Task<RelationshipResponseDto> UpdateRelationship(long projectId, long relationshipId, RelationshipRequestDto dto);
    Task<bool> DeleteRelationship(long projectId, long relationshipId);
    Task<bool> BulkSoftDeleteRelationships(string domainType, IEnumerable<long> domainIds);
}