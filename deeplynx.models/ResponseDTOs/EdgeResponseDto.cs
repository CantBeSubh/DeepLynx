using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class EdgeResponseDto
{
    [Column("id")]
    public long Id { get; set; }
    [Column("origin_id")]
    public long OriginId { get; set; } 
    [Column("destination_id")]
    public long DestinationId { get; set; }
    [Column("relationship_id")]
    public long? RelationshipId { get; set; }
    [Column("mapping_id")]
    public long? MappingId { get; set; }
    [Column("data_source_id")]
    public long DataSourceId { get; set; }
    [Column("project_id")]
    public long ProjectId { get; set; }
    [Column("created_by")]
    public string? CreatedBy { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }
    [Column("archived_at")]
    public DateTime? ArchivedAt { get; set; }
}