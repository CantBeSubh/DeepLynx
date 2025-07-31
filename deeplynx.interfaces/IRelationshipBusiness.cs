using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore.Storage;

namespace deeplynx.interfaces;

public interface IRelationshipBusiness
{
    Task<List<RelationshipResponseDto>> GetAllRelationships(long projectId, bool hideArchived);
    Task<RelationshipResponseDto> GetRelationship(long projectId, long relationshipId, bool hideArchived);
    Task<List<RelationshipResponseDto>> BulkCreateRelationships(long projectId, List<CreateRelationshipRequestDto> relationshipRequestDtos);
    Task<RelationshipResponseDto> CreateRelationship(long projectId, CreateRelationshipRequestDto dto);
    Task<RelationshipResponseDto> UpdateRelationship(long projectId, long relationshipId, UpdateRelationshipRequestDto dto);
    Task<bool> DeleteRelationship(long projectId, long relationshipId);
    Task<bool> ArchiveRelationship(long projectId, long relationshipId);
    Task<bool> UnarchiveRelationship(long projectId, long relationshipId);
}