using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("tags", Schema = "deeplynx")]
[Index("Id", Name = "idx_tags_id")]
[Index("Name", Name = "idx_tags_name")]
[Index("ProjectId", Name = "idx_tags_project_id")]
[Index(nameof(ProjectId), nameof(Name), IsUnique = true, Name = "unique_tag_name")]
public partial class Tag
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

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

    [ForeignKey("ProjectId")]
    [InverseProperty("Tags")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("Tags")]
    public virtual ICollection<RecordMapping> RecordMappings { get; set; } = new List<RecordMapping>();

    [ForeignKey("TagId")]
    [InverseProperty("Tags")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();
}
