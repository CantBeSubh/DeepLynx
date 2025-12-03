using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.interfaces;

public interface IMetadataBusiness
{
    Task<MetadataResponseDto> CreateMetadata(long currentUserId, long projectId, long organizationId, long dataSourceId,
        CreateMetadataRequestDto metadataRequestDto);

    Task<MetadataResponseDto> CreateMetadataFromFile(long currentUserId, long projectId, long organizationId,
        long dataSourceId, IFormFile file);
}