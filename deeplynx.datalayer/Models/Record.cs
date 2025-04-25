using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("records", Schema = "deeplynx")]
public partial class Record
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uri")]
    public string? Uri { get; set; }

    [Column("properties", TypeName = "jsonb")]
    public string Properties { get; set; } = null!;

    [Column("original_id")]
    public string? OriginalId { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("custom_id")]
    public string? CustomId { get; set; }

    [Column("class_id")]
    public long? ClassId { get; set; }

    [Column("class_name")]
    public string? ClassName { get; set; }

    [Column("data_source_id")]
    public long DataSourceId { get; set; }

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

    [ForeignKey("ClassId")]
    [InverseProperty("Records")]
    public virtual Class? Class { get; set; }

    [ForeignKey("DataSourceId")]
    [InverseProperty("Records")]
    public virtual DataSource DataSource { get; set; } = null!;

    [InverseProperty("Destination")]
    public virtual ICollection<Edge> EdgeDestinations { get; set; } = new List<Edge>();

    [InverseProperty("Origin")]
    public virtual ICollection<Edge> EdgeOrigins { get; set; } = new List<Edge>();

    [InverseProperty("Record")]
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    [ForeignKey("ProjectId")]
    [InverseProperty("Records")]
    public virtual Project Project { get; set; } = null!;
}
