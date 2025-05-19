using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRelationshipBusiness
{
    Task<IEnumerable<Relationship>> GetAllRelationships(long projectId);
    Task<Relationship> GetRelationship(long projectId, long relationshipId);
    Task<Relationship> CreateRelationship(long projectId, RelationshipRequestDto dto);
    Task<Relationship> UpdateRelationship(long projectId, long relationshipId, RelationshipRequestDto dto);
    Task<bool> DeleteRelationship(long projectId, long relationshipId);
    Task<bool> SoftDeleteAllRelationshipsByProjectIdAsync(long projectId);
}