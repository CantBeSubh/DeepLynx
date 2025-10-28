using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("relationships", Schema = "deeplynx")]
[Index("DestinationId", Name = "idx_relationships_destination_id")]
[Index("Id", Name = "idx_relationships_id")]
[Index("Name", Name = "idx_relationships_name")]
[Index("OriginId", Name = "idx_relationships_origin_id")]
[Index("ProjectId", Name = "idx_relationships_project_id")]
[Index("Uuid", Name = "idx_relationships_uuid")]
[Index("ProjectId", "Name", Name = "unique_relationship_name", IsUnique = true)]
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
    public long? OriginId { get; set; }

    [Column("destination_id")]
    public long? DestinationId { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [ForeignKey("DestinationId")]
    [InverseProperty("RelationshipDestinations")]
    public virtual Class? Destination { get; set; }

    [InverseProperty("Relationship")]
    public virtual ICollection<Edge> Edges { get; set; } = new List<Edge>();

    [ForeignKey("OriginId")]
    [InverseProperty("RelationshipOrigins")]
    public virtual Class? Origin { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Relationships")]
    public virtual Project Project { get; set; } = null!;
    
    [InverseProperty("UpdatedRelationships")]
    public virtual User? LastUpdatedByUser { get; set; }
}
