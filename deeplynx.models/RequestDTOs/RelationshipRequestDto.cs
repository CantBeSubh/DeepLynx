using System.ComponentModel.DataAnnotations;

namespace deeplynx.models;

public class RelationshipRequestDto
{
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Uuid { get; set; }
    
    [Required]
    public long OriginId { get; set; }
    
    [Required]
    public long DestinationId { get; set; }
}