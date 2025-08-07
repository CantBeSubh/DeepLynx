using System.Text.Json.Serialization;
    
namespace deeplynx.models;

public class UpdateTagRequestDto
{
    public string? Name { get; set; } = null!;
    public string? CreatedBy { get; set; }
    [JsonIgnore]
    public DateTime? CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    [JsonIgnore]
    public DateTime? ModifiedAt { get; set; }
    [JsonIgnore]
    public DateTime? ArchivedAt { get; set; }
}