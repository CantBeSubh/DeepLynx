using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class EdgeRequestDto
{
    [JsonPropertyName("origin_id")]
    public long? OriginId { get; set; } 
    
    [JsonPropertyName("destination_id")]
    public long? DestinationId { get; set; }
    
    [JsonPropertyName("relationship_id")]
    public long? RelationshipId { get; set; }
    
    [JsonPropertyName("relationship_name")]
    public string? RelationshipName { get; set; }
    
    [JsonPropertyName("origin_oid")]
    public string? OriginOid { get; set; } 
    
    [JsonPropertyName("destination_oid")]
    public string? DestinationOid { get; set; }
}