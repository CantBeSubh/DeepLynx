using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("object_storage", Schema = "deeplynx")]
[Index("Id", Name = "idx_object_storage_id")]
public partial class ObjectStorage
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;
    
    [Column("type")]
    public string Type { get; set; } = null!;
    
    [Column("config", TypeName = "jsonb")]
    public string Config { get; set; } = null!;

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

    [Column("archived_at", TypeName = "timestamp without time zone")]
    public DateTime? ArchivedAt { get; set; }
    
    [ForeignKey("ProjectId")]
    [InverseProperty("ObjectStorages")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("ObjectStorage")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();
}