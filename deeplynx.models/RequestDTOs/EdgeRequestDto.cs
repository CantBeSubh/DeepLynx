using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class EdgeRequestDto
{
    [Required]
    public long OriginId { get; set; } 
    
    [Required]
    public long DestinationId { get; set; }
    
    public long? RelationshipId { get; set; }
    
    public string? RelationshipName { get; set; }
}