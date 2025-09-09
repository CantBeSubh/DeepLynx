using System.Text.Json.Nodes;

namespace deeplynx.models;

public class EdgeMappingResponseDto
{
    public long Id { get; set; }
    public JsonObject? OriginParams { get; set; }
    public JsonObject? DestinationParams { get; set; }
    public long RelationshipId { get; set; }
    public long OriginId { get; set; }
    public long DestinationId { get; set; }
    public long DataSourceId { get; set; }
    public long ProjectId { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
    public bool IsArchived { get; set; } = false;
}