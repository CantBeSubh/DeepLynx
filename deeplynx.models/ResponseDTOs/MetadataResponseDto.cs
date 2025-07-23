using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
    
namespace deeplynx.models;

public class MetadataResponseDto
{
    public long? Id { get; set; }
    public long ProjectId { get; set; }
    
    public long DataSourceId { get; set; }
    public List<ClassResponseDto>? Classes { get; set; }
    public List<RelationshipResponseDto>? Relationships { get; set; }
    public List<TagResponseDto>? Tags { get; set; }
    public List<RecordResponseDto>? Records { get; set; }
    public List<EdgeResponseDto>? Edges { get; set; }
    public string? CreatedBy { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    [JsonIgnore]
    public DateTime? ModifiedAt { get; set; }
    [JsonIgnore]
    public DateTime? ArchivedAt { get; set; }
}