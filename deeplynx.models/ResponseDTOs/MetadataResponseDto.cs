namespace deeplynx.models;

public class MetadataResponseDto
{
    public IEnumerable<ClassResponseDto>? Classes { get; set; }
    public IEnumerable<RelationshipResponseDto>? Relationships { get; set; }
    public IEnumerable<TagResponseDto>? Tags { get; set; }
    public IEnumerable<RecordResponseDto>? Records { get; set; }
    public IEnumerable<EdgeResponseDto>? Edges { get; set; }
}