using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class EdgeRequestDto
{
    [Required]
    public int OriginId { get; set; } 
    
    [Required]
    public int DestinationId { get; set; }
    
    public int? RelationshipId { get; set; }
    
    public string? RelationshipName { get; set; }
}