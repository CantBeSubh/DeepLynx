using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("classes", Schema = "deeplynx")]
[Index("ProjectId", Name = "IX_classes_project_id")]
[Index("Id", Name = "idx_classes_id")]
[Index("ProjectId", Name = "idx_classes_project_id")]
[Index("Uuid", Name = "idx_classes_uuid")]
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

    [InverseProperty("Destination")]
    public virtual ICollection<EdgeParameter> EdgeParameterDestinations { get; set; } = new List<EdgeParameter>();

    [InverseProperty("Origin")]
    public virtual ICollection<EdgeParameter> EdgeParameterOrigins { get; set; } = new List<EdgeParameter>();

    [ForeignKey("ProjectId")]
    [InverseProperty("Classes")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("Class")]
    public virtual ICollection<RecordParameter> RecordParameters { get; set; } = new List<RecordParameter>();

    [InverseProperty("Class")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    [InverseProperty("Destination")]
    public virtual ICollection<Relationship> RelationshipDestinations { get; set; } = new List<Relationship>();

    [InverseProperty("Origin")]
    public virtual ICollection<Relationship> RelationshipOrigins { get; set; } = new List<Relationship>();
}
