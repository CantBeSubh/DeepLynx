using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("edge_mappings", Schema = "deeplynx")]
[Index("DestinationId", Name = "idx_edge_mappings_destination_id")]
[Index("Id", Name = "idx_edge_mappings_id")]
[Index("OriginId", Name = "idx_edge_mappings_origin_id")]
[Index("ProjectId", Name = "idx_edge_mappings_project_id")]
[Index("RelationshipId", Name = "idx_edge_mappings_relationship_id")]
public partial class EdgeMapping
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("origin_params", TypeName = "jsonb")]
    public string OriginParams { get; set; } = null!;

    [Column("destination_params", TypeName = "jsonb")]
    public string DestinationParams { get; set; } = null!;

    [Column("relationship_id")]
    public long RelationshipId { get; set; }

    [Column("origin_id")]
    public long OriginId { get; set; }

    [Column("destination_id")]
    public long DestinationId { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }

    [Column("modified_at", TypeName = "timestamp without time zone")]
    public DateTime? ModifiedAt { get; set; }

    [Column("deleted_at", TypeName = "timestamp without time zone")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("DestinationId")]
    [InverseProperty("EdgeMappingDestinations")]
    public virtual Class Destination { get; set; } = null!;

    [ForeignKey("OriginId")]
    [InverseProperty("EdgeMappingOrigins")]
    public virtual Class Origin { get; set; } = null!;

    [ForeignKey("ProjectId")]
    [InverseProperty("EdgeMappings")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("RelationshipId")]
    [InverseProperty("EdgeMappings")]
    public virtual Relationship Relationship { get; set; } = null!;
}
