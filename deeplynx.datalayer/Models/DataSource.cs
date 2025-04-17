using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("data_sources", Schema = "deeplynx")]
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

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }

    [Column("modified_at", TypeName = "timestamp without time zone")]
    public DateTime ModifiedAt { get; set; }

    [Column("deleted_at", TypeName = "timestamp without time zone")]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("DataSource")]
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    [ForeignKey("ProjectId")]
    [InverseProperty("DataSources")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("DataSource")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();
}
