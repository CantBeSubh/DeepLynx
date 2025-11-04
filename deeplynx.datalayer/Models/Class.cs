using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("classes", Schema = "deeplynx")]
[Index("Id", Name = "idx_classes_id")]
[Index("Name", Name = "idx_classes_name")]
[Index("ProjectId", Name = "idx_classes_project_id")]
[Index("Uuid", Name = "idx_classes_uuid")]
[Index("ProjectId", "Name", Name = "unique_class_name", IsUnique = true)]
public partial class Class
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("uuid")]
    public string? Uuid { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("last_updated_by")]
    public long? LastUpdatedBy { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Classes")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("Class")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    [InverseProperty("Destination")]
    public virtual ICollection<Relationship> RelationshipDestinations { get; set; } = new List<Relationship>();

    [InverseProperty("Origin")]
    public virtual ICollection<Relationship> RelationshipOrigins { get; set; } = new List<Relationship>();

    [InverseProperty("LastUpdatedClasses")]
    public virtual User? LastUpdatedByUser { get; set; }

}
