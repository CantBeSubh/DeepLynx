using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("relationships", Schema = "deeplynx")]
[Index("DestinationId", Name = "idx_relationships_destination_id")]
[Index("Id", Name = "idx_relationships_id")]
[Index("OriginId", Name = "idx_relationships_origin_id")]
[Index("ProjectId", Name = "idx_relationships_project_id")]
[Index("Uuid", Name = "idx_relationships_uuid")]
public partial class Relationship
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("uuid")]
    public string? Uuid { get; set; }

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

    [Column("archived_at", TypeName = "timestamp without time zone")]
    public DateTime? ArchivedAt { get; set; }

    [ForeignKey("DestinationId")]
    [InverseProperty("RelationshipDestinations")]
    public virtual Class Destination { get; set; } = null!;

    [InverseProperty("Relationship")]
    public virtual ICollection<EdgeMapping> EdgeMappings { get; set; } = new List<EdgeMapping>();

    [InverseProperty("Relationship")]
    public virtual ICollection<Edge> Edges { get; set; } = new List<Edge>();

    [ForeignKey("OriginId")]
    [InverseProperty("RelationshipOrigins")]
    public virtual Class Origin { get; set; } = null!;

    [ForeignKey("ProjectId")]
    [InverseProperty("Relationships")]
    public virtual Project Project { get; set; } = null!;
}
