using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("tags", Schema = "deeplynx")]
[Index("Id", Name = "idx_tags_id")]
[Index("ProjectId", Name = "idx_tags_project_id")]
public partial class Tag
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

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

    [Column("deleted_at", TypeName = "timestamp without time zone")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Tags")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("Tag")]
    public virtual ICollection<RoleResource> RoleResources { get; set; } = new List<RoleResource>();
    
    [InverseProperty("Tag")]
    public virtual ICollection<RecordMapping> RecordMappings { get; set; } = new List<RecordMapping>();

    [ForeignKey("TagId")]
    [InverseProperty("Tags")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();
}
