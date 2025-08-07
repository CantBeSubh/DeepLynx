using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace deeplynx.models;

public class UpdateEdgeMappingRequestDto
{
    public JsonObject? OriginParams { get; set; }
    
    public JsonObject? DestinationParams { get; set; }
    
    public long? DataSourceId { get; set; }

    public long? RelationshipId { get; set; }
    
    public long? OriginId { get; set; }
    
    public long? DestinationId { get; set; }
}