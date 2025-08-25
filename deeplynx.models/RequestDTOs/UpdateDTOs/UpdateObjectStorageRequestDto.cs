using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class UpdateObjectStorageRequestDto
{
    [JsonPropertyName("name")] 
    public string Name { get; set; } = null!;
}