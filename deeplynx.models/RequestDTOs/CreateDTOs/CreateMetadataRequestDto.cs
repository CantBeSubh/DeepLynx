namespace deeplynx.models;

public class CreateMetadataRequestDto
{
    public List<CreateClassRequestDto>? Classes { get; set; }
    public List<CreateRelationshipRequestDto>? Relationships { get; set; }
    public List<CreateTagRequestDto>? Tags { get; set; }
    public List<CreateRecordRequestDto>? Records { get; set; }
    public List<CreateEdgeRequestDto>? Edges { get; set; }
}