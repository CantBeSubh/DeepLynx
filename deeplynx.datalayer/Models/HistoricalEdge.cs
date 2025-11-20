using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.datalayer.Models;

[Table("historical_edges", Schema = "deeplynx")]
public partial class HistoricalEdge
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("edge_id")]
    public long EdgeId { get; set; }

    [Column("origin_id")]
    public long OriginId { get; set; }

    [Column("destination_id")]
    public long DestinationId { get; set; }

    [Column("relationship_id")]
    public long? RelationshipId { get; set; }

    [Column("relationship_name")]
    public string? RelationshipName { get; set; }

    [Column("data_source_id")]
    public long DataSourceId { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }
    
    [Column("organization_id")]
    public long OrganizationId { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("data_source_name")]
    public string DataSourceName { get; set; } = null!;

    [Column("project_name")]
    public string ProjectName { get; set; } = null!;

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [ForeignKey("EdgeId")]
    [InverseProperty("HistoricalEdges")]
    public virtual Edge Edge { get; set; } = null!;
    
    [ForeignKey("ProjectId")]
    [InverseProperty("HistoricalEdges")]
    public virtual Project Project { get; set; } = null!;
    
    [ForeignKey("OrganizationId")]
    [InverseProperty("HistoricalEdges")]
    public virtual Organization Organization { get; set; } = null!;
    
    [InverseProperty("LastUpdatedHistoricalEdges")]
    public virtual User? LastUpdatedByUser { get; set; }
}
