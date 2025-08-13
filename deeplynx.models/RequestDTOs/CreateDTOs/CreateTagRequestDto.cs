using System.Text.Json.Serialization;
    
namespace deeplynx.models;

public class CreateTagRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}