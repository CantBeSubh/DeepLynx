using System.Text.Json.Serialization;
    
namespace deeplynx.models;

public class TagRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}