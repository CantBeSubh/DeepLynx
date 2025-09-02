using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("classes", Schema = "deeplynx")]
[Index("Id", Name = "idx_classes_id")]
[Index("ProjectId", Name = "idx_classes_project_id")]
[Index("Uuid", Name = "idx_classes_uuid")]
[Index("Name", Name = "idx_classes_name")]
[Index(nameof(ProjectId), nameof(Name), IsUnique = true, Name = "unique_class_name")]
public partial class Class
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("uuid")]
    public string? Uuid { get; set; }

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

    [InverseProperty("Destination")]
    public virtual ICollection<EdgeMapping> EdgeMappingDestinations { get; set; } = new List<EdgeMapping>();

    [InverseProperty("Origin")]
    public virtual ICollection<EdgeMapping> EdgeMappingOrigins { get; set; } = new List<EdgeMapping>();

    [ForeignKey("ProjectId")]
    [InverseProperty("Classes")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("Class")]
    public virtual ICollection<RecordMapping> RecordMappings { get; set; } = new List<RecordMapping>();

    [InverseProperty("Class")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    [InverseProperty("Destination")]
    public virtual ICollection<Relationship> RelationshipDestinations { get; set; } = new List<Relationship>();

    [InverseProperty("Origin")]
    public virtual ICollection<Relationship> RelationshipOrigins { get; set; } = new List<Relationship>();
}
