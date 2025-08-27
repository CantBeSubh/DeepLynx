using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("data_sources", Schema = "deeplynx")]
[Index("Id", Name = "idx_data_sources_id")]
[Index("ProjectId", Name = "idx_data_sources_project_id")]
public partial class DataSource
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;
    
    [Column("description")]
    public string? Description { get; set; }

    [Column("abbreviation")]
    public string? Abbreviation { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("base_uri")]
    public string? BaseUri { get; set; }

    [Column("config", TypeName = "jsonb")]
    public string? Config { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }

    [Required]
    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }
    
    [Column("last_updated_by")]
    public string? LastUpdatedBy { get; set; }
    
    [Required]
    [Column("is_archived")]
    public bool IsArchived { get; set; } = false;
    [InverseProperty("DataSource")]
    public virtual ICollection<Edge> Edges { get; set; } = new List<Edge>();
    
    [InverseProperty("DataSource")]
    public virtual ICollection<RecordMapping> RecordMappings { get; set; } = new List<RecordMapping>();
    
    [InverseProperty("DataSource")]
    public virtual ICollection<EdgeMapping> EdgeMappings { get; set; } = new List<EdgeMapping>();

    [ForeignKey("ProjectId")]
    [InverseProperty("DataSources")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("DataSource")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    [InverseProperty("DataSource")]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    
    [InverseProperty("DataSource")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
