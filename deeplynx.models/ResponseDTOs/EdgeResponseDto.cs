using System.Text.Json.Nodes;

namespace deeplynx.models;

public class EdgeResponseDto
{
    public long Id { get; set; }
    public long OriginId { get; set; } 
    public long DestinationId { get; set; }
    public long? RelationshipId { get; set; }
    public long? MappingId { get; set; }
    public long DataSourceId { get; set; }
    public long ProjectId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
}