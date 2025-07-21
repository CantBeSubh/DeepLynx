using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
    
namespace deeplynx.models;

public class MetadataResponseDto
{
    public long? Id { get; set; }
    public long ProjectId { get; set; }
    
    public long DataSourceId { get; set; }
    public BulkClassResponseDto? Classes { get; set; }
    public BulkRelationshipResponseDto? Relationships { get; set; }
    public BulkTagResponseDto? Tags { get; set; }
    public BulkRecordResponseDto? Records { get; set; }
    public BulkEdgeResponseDto? Edges { get; set; }
    public string? CreatedBy { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    [JsonIgnore]
    public DateTime? ModifiedAt { get; set; }
    [JsonIgnore]
    public DateTime? ArchivedAt { get; set; }
}