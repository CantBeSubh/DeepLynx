using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("projects", Schema = "deeplynx")]
public partial class Project
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("abbreviation")]
    public string? Abbreviation { get; set; }

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

    [InverseProperty("Project")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    [InverseProperty("Project")]
    public virtual ICollection<DataSource> DataSources { get; set; } = new List<DataSource>();

    [InverseProperty("Project")]
    public virtual ICollection<EdgeParameter> EdgeParameters { get; set; } = new List<EdgeParameter>();

    [InverseProperty("Project")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    [InverseProperty("Project")]
    public virtual ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();

    [InverseProperty("Project")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [ForeignKey("ProjectId")]
    [InverseProperty("Projects")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
