using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
    
namespace deeplynx.models;

public class MetadataResponseDto
{
    
    public List<ClassResponseDto>? Classes { get; set; }
    public List<RelationshipResponseDto>? Relationships { get; set; }
    public List<TagResponseDto>? Tags { get; set; }
    public List<RecordResponseDto>? Records { get; set; }
    public List<EdgeResponseDto>? Edges { get; set; }
}