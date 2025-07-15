using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
    
namespace deeplynx.models;

public class MetadataResponseDto
{
    public long? Id { get; set; }
    public long ProjectId { get; set; }
    public JsonArray? Classes { get; set; }
    public JsonArray? Relationships { get; set; }
    public JsonArray? Tags { get; set; }
    public JsonArray? Records { get; set; }
    public JsonArray? Edges { get; set; }
    public string? CreatedBy { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    [JsonIgnore]
    public DateTime? ModifiedAt { get; set; }
    [JsonIgnore]
    public DateTime? ArchivedAt { get; set; }
}