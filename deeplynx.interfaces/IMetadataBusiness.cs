using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.interfaces;

public interface IMetadataBusiness
{ 
    Task<MetadataResponseDto> CreateMetadata(long projectId, long dataSourceId, CreateMetadataRequestDto metadataRequestDto);
    Task<MetadataResponseDto> CreateMetadataFromFile(long projectId, long dataSourceId, IFormFile file);
}