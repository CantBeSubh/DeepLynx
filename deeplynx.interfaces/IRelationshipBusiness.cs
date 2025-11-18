using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface IRelationshipBusiness
{
    Task<List<RelationshipResponseDto>> GetAllRelationships(long projectId, bool hideArchived);
    Task<RelationshipResponseDto> GetRelationship(long projectId, long relationshipId, bool hideArchived);
    Task<List<RelationshipResponseDto>> BulkCreateRelationships(long currentUserId, long projectId, List<CreateRelationshipRequestDto> relationshipRequestDtos);
    Task<RelationshipResponseDto> CreateRelationship(long currentUserId, long projectId, CreateRelationshipRequestDto dto);
    Task<RelationshipResponseDto> UpdateRelationship(long currentUserId, long projectId, long relationshipId, UpdateRelationshipRequestDto dto);
    Task<bool> DeleteRelationship(long projectId, long relationshipId);
    Task<bool> ArchiveRelationship(long currentUserId, long projectId, long relationshipId);
    Task<bool> UnarchiveRelationship(long currentUserId, long projectId, long relationshipId);
    Task<List<RelationshipResponseDto>> GetRelationshipsByName(long projectId, List<string> relationshipNames);

}