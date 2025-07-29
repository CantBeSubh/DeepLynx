using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class EdgeRequestDto
{
    [Required]
    [JsonPropertyName("origin_id")]
    public long OriginId { get; set; } 
    
    [Required]
    [JsonPropertyName("destination_id")]
    public long DestinationId { get; set; }
    
    [JsonPropertyName("relationship_id")]
    public long? RelationshipId { get; set; }
    
    [JsonPropertyName("relationship_name")]
    public string? RelationshipName { get; set; }
    
    
}