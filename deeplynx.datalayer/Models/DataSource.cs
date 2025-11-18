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
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

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

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("default")]
    public bool Default { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [InverseProperty("DataSource")]
    public virtual ICollection<Edge> Edges { get; set; } = new List<Edge>();

    [ForeignKey("ProjectId")]
    [InverseProperty("DataSources")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("DataSource")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    [InverseProperty("DataSource")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    [InverseProperty("DataSource")]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    
    [InverseProperty("LastUpdatedDataSources")]
    public virtual User? LastUpdatedByUser { get; set; }
}
