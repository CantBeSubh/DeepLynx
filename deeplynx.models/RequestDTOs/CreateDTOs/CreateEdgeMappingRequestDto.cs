using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class CreateEdgeMappingRequestDto
{
    [Required]
    public JsonObject OriginParams { get; set; }
    
    [Required]
    public JsonObject DestinationParams { get; set; }
    
    [Required]
    public long DataSourceId { get; set; }

    public long RelationshipId { get; set; }
    
    public long OriginId { get; set; }
    
    public long DestinationId { get; set; }
}