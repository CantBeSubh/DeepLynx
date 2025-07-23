using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class ClassRequestDto : AbstractNexusCoreDataRequestDto
{
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    public string? Uuid { get; set; }

}