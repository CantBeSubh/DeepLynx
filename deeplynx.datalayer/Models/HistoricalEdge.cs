using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

// Note that foreign keys other than edge_id are absent- this is because for historical records,
// foreign keys are declarative and informative instead of referential in nature.

[Table("historical_edges", Schema = "deeplynx")]
[Index("DestinationId", Name = "idx_historical_edges_destination_id")]
[Index("OriginId", Name = "idx_historical_edges_origin_id")]
[Index("EdgeId", Name = "idx_historical_edges_edge_id")]
[Index("RelationshipName", Name = "idx_historical_edges_relationship_name")]
[Index("Id", Name = "idx_historical_edges_id")]
[Index("Current", Name = "idx_historical_edges_current")]
[Index("LastUpdatedAt", Name = "idx_historical_edges_last_updated_at")]
public partial class HistoricalEdge
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    [Column("edge_id")]
    public long? EdgeId { get; set; }
    
    [Column("origin_id")]
    public long OriginId { get; set; }
    
    [Column("destination_id")]
    public long DestinationId { get; set; }

    [Column("relationship_id")]
    public long? RelationshipId { get; set; }

    [Column("relationship_name")]
    public string? RelationshipName { get; set; }

    [Column("mapping_id")]
    public long? MappingId { get; set; }

    [Column("data_source_id")]
    public long DataSourceId { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }
    
    [Column("current")]
    public bool Current { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }

    [Column("modified_at", TypeName = "timestamp without time zone")]
    public DateTime? ModifiedAt { get; set; }

    [Column("archived_at", TypeName = "timestamp without time zone")]
    public DateTime? ArchivedAt { get; set; }

    [Column("deleted_at", TypeName = "timestamp without time zone")]
    public DateTime? DeletedAt { get; set; }
    
    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime? LastUpdatedAt { get; set; }
    
    [ForeignKey("EdgeId")]
    [InverseProperty("HistoricalEdges")]
    public virtual Edge? Edge { get; set; }
}