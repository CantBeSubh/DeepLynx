using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("historical_edges", Schema = "deeplynx")]
[Index("DestinationId", Name = "idx_historical_edges_destination_id")]
[Index("EdgeId", Name = "idx_historical_edges_edge_id")]
[Index("Id", Name = "idx_historical_edges_id")]
[Index("LastUpdatedAt", Name = "idx_historical_edges_last_updated_at")]
[Index("OriginId", Name = "idx_historical_edges_origin_id")]
[Index("RelationshipName", Name = "idx_historical_edges_relationship_name")]
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

    [Column("last_updated_by")]
    public string? LastUpdatedBy { get; set; }

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
}
