using deeplynx.models;

namespace deeplynx.interfaces;

public interface IMetadataBusiness
{
    Task<MetadataResponseDto> CreateMetadata(long projectId, MetadataRequestDto metadataRequestDto);
}