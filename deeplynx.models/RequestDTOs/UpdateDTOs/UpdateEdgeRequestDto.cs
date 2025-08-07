using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class UpdateEdgeRequestDto
{
    [JsonPropertyName("origin_id")]
    public long? OriginId { get; set; }
    
    [JsonPropertyName("destination_id")]
    public long? DestinationId { get; set; }
    
    [JsonPropertyName("name")]
    public string? RelationshipName { get; set; }
    
    public long? RelationshipId { get; set; }
}