using System.Text.Json.Serialization;
    
namespace deeplynx.models;

public class TagRequestDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long ProjectId { get; set; }
    public string? CreatedBy { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    [JsonIgnore]
    public DateTime? ModifiedAt { get; set; }
    [JsonIgnore]
    public DateTime? ArchivedAt { get; set; }
}