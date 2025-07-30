using deeplynx.models;

namespace deeplynx.interfaces;

public interface IMetadataBusiness
{
    Task<MetadataResponseDto> CreateMetadata(long projectId, long dataSourceId, MetadataRequestDto metadataRequestDto);
}