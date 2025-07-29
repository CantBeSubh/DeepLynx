using System.Text.Json.Serialization;

namespace deeplynx.models;

public class BulkMetadataRequestDto
{
    public long? Id { get; set; }
    public BulkClassRequestDto? Classes { get; set; }
    public BulkRelationshipRequestDto? Relationships { get; set; }
    public BulkTagRequestDto? Tags { get; set; }
    public BulkEdgeRequestDto? Edges { get; set; }
    public string? CreatedBy { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    [JsonIgnore]
    public DateTime? ModifiedAt { get; set; }
    [JsonIgnore]
    public DateTime? ArchivedAt { get; set; }
}