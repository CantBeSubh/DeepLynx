using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[PrimaryKey("OriginId", "DestinationId")]
[Table("edges", Schema = "deeplynx")]
public partial class Edge
{
    [Key]
    [Column("origin_id")]
    public long OriginId { get; set; }

    [Key]
    [Column("destination_id")]
    public long DestinationId { get; set; }

    [Column("properties", TypeName = "jsonb")]
    public string? Properties { get; set; }

    [Column("relationship_id")]
    public long? RelationshipId { get; set; }

    [Column("relationship_name")]
    public string? RelationshipName { get; set; }

    [ForeignKey("DestinationId")]
    [InverseProperty("EdgeDestinations")]
    public virtual Record Destination { get; set; } = null!;

    [ForeignKey("OriginId")]
    [InverseProperty("EdgeOrigins")]
    public virtual Record Origin { get; set; } = null!;

    [ForeignKey("RelationshipId")]
    [InverseProperty("Edges")]
    public virtual Relationship? Relationship { get; set; }
}
