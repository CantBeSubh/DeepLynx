using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class RelationshipRequestDto
{
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Uuid { get; set; }
    
    public long? OriginId { get; set; }
    
    public long? DestinationId { get; set; }
}