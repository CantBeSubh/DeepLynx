using System.Text.Json.Serialization;

namespace deeplynx.models;

public class RelationshipRequestDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Uuid { get; set; }

    [JsonPropertyName("origin_class")]
    public string OriginClass { get; set; } = null!;

    [JsonPropertyName("destination_class")]
    public string DestinationClass { get; set; } = null!;
}