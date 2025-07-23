using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class EdgeRequestDto :  AbstractNexusCoreDataRequestDto
{
    [Required]
    [JsonPropertyName("origin_id")]
    public long OriginId { get; set; } 
    
    [Required]
    [JsonPropertyName("destination_id")]
    public long DestinationId { get; set; }
    
    [JsonPropertyName("name")]
    public string? RelationshipName { get; set; }
    
    public long? RelationshipId { get; set; }
}