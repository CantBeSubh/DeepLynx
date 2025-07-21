using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("projects", Schema = "deeplynx")]
[Index("Id", Name = "idx_projects_id")]
public partial class Project
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("abbreviation")]
    public string? Abbreviation { get; set; }
    
    [Column("description")]
    public string? Description { get; set; }

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

    [InverseProperty("Project")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    [InverseProperty("Project")]
    public virtual ICollection<DataSource> DataSources { get; set; } = new List<DataSource>();

    [InverseProperty("Project")]
    public virtual ICollection<EdgeMapping> EdgeMappings { get; set; } = new List<EdgeMapping>();

    [InverseProperty("Project")]
    public virtual ICollection<Edge> Edges { get; set; } = new List<Edge>();

    [InverseProperty("Project")]
    public virtual ICollection<RecordMapping> RecordMappings { get; set; } = new List<RecordMapping>();

    [InverseProperty("Project")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    [InverseProperty("Project")]
    public virtual ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();

    [InverseProperty("Project")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    
    [InverseProperty("Project")]
    public virtual ICollection<ObjectStorage> ObjectStorages { get; set; } = new List<ObjectStorage>();
    
    [InverseProperty("Projects")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

}
