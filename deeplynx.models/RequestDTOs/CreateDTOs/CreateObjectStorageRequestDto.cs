using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class CreateObjectStorageRequestDto
{
    [Required] [JsonPropertyName("name")] public string Name { get; set; }

    [Required]
    [JsonPropertyName("config")]
    public JsonObject Config { get; set; }

    [JsonPropertyName("default")] public bool Default { get; set; } = false;
}