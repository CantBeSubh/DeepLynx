using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[PrimaryKey("OriginId", "DestinationId")]
[Table("edges", Schema = "deeplynx")]
[Index("DestinationId", Name = "IX_edges_destination_id")]
[Index("RelationshipId", Name = "IX_edges_relationship_id")]
[Index("DestinationId", Name = "idx_edges_destination_id")]
[Index("OriginId", Name = "idx_edges_origin_id")]
[Index("RelationshipId", Name = "idx_edges_relationship_id")]
[Index("ProjectId", Name = "idx_edges_project_id")]
[Index("DataSourceId", Name = "idx_edges_data_source_id")]
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
    
    [Column("data_source_id")]
    public long? DataSourceId { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }

    [ForeignKey("DestinationId")]
    [InverseProperty("EdgeDestinations")]
    public virtual Record Destination { get; set; } = null!;

    [ForeignKey("OriginId")]
    [InverseProperty("EdgeOrigins")]
    public virtual Record Origin { get; set; } = null!;

    [ForeignKey("RelationshipId")]
    [InverseProperty("Edges")]
    public virtual Relationship? Relationship { get; set; }
    
    [ForeignKey("DataSourceId")]
    [InverseProperty("Edges")]
    public virtual DataSource? DataSource { get; set; }
    
    [ForeignKey("ProjectId")]
    [InverseProperty("Edges")]
    public virtual Project Project { get; set; } = null!;
}
