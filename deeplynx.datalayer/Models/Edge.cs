using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("edges", Schema = "deeplynx")]
[Index("DataSourceId", Name = "idx_edges_data_source_id")]
[Index("DestinationId", Name = "idx_edges_destination_id")]
[Index("Id", Name = "idx_edges_id")]
[Index("MappingId", Name = "idx_edges_mapping_id")]
[Index("OriginId", Name = "idx_edges_origin_id")]
[Index("ProjectId", Name = "idx_edges_project_id")]
[Index("RelationshipId", Name = "idx_edges_relationship_id")]
[Index("ProjectId", "OriginId", "DestinationId", Name = "unique_edge_record_ids", IsUnique = true)]
public partial class Edge
{
    [Column("origin_id")]
    public long OriginId { get; set; }

    [Column("destination_id")]
    public long DestinationId { get; set; }

    [Column("relationship_id")]
    public long? RelationshipId { get; set; }

    [Column("data_source_id")]
    public long DataSourceId { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("last_updated_by")]
    public string? LastUpdatedBy { get; set; }

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("mapping_id")]
    public long? MappingId { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [ForeignKey("DataSourceId")]
    [InverseProperty("Edges")]
    public virtual DataSource DataSource { get; set; } = null!;

    [ForeignKey("DestinationId")]
    [InverseProperty("EdgeDestinations")]
    public virtual Record Destination { get; set; } = null!;

    [InverseProperty("Edge")]
    public virtual ICollection<HistoricalEdge> HistoricalEdges { get; set; } = new List<HistoricalEdge>();

    [ForeignKey("MappingId")]
    [InverseProperty("Edges")]
    public virtual EdgeMapping? Mapping { get; set; }

    [ForeignKey("OriginId")]
    [InverseProperty("EdgeOrigins")]
    public virtual Record Origin { get; set; } = null!;

    [ForeignKey("ProjectId")]
    [InverseProperty("Edges")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("RelationshipId")]
    [InverseProperty("Edges")]
    public virtual Relationship? Relationship { get; set; }
}
