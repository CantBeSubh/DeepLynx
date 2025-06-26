using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface IRelationshipBusiness
{
    Task<IEnumerable<RelationshipResponseDto>> GetAllRelationships(long projectId);
    Task<RelationshipResponseDto> GetRelationship(long projectId, long relationshipId);
    Task<RelationshipResponseDto> CreateRelationship(long projectId, RelationshipRequestDto dto);
    Task<RelationshipResponseDto> UpdateRelationship(long projectId, long relationshipId, RelationshipRequestDto dto);
    Task<bool> DeleteRelationship(long projectId, long relationshipId);
    Task<bool> ArchiveRelationship(long projectId, long relationshipId);
}