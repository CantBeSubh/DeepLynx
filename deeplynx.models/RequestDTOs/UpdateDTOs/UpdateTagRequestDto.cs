using System.Text.Json.Serialization;
    
namespace deeplynx.models;

public class UpdateTagRequestDto
{
    public string? Name { get; set; } = null!;
    [JsonIgnore]
    public string? LastUpdatedBy { get; set; }
    [JsonIgnore]
    public DateTime? LastUpdatedAt { get; set; }
}