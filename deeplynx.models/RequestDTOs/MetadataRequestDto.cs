using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
    
namespace deeplynx.models;

public class MetadataRequestDto
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public JsonObject? Classes { get; set; }
    public JsonObject? Relationships { get; set; }
    public JsonObject? Tags { get; set; }
    public JsonObject? Records { get; set; }
    public JsonObject? Edges { get; set; }
    public string? CreatedBy { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    [JsonIgnore]
    public DateTime? ModifiedAt { get; set; }
    [JsonIgnore]
    public DateTime? ArchivedAt { get; set; }
}