using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.interfaces;

public interface IMetadataBusiness
{
    Task<MetadataResponseDto> CreateMetadata(long organizationId, long projectId, long dataSourceId, CreateMetadataRequestDto metadataRequestDto);
    Task<MetadataResponseDto> CreateMetadataFromFile(long organizationId, long projectId, long dataSourceId, IFormFile file);
}