using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("record_mappings", Schema = "deeplynx")]
[Index("ClassId", Name = "idx_record_mappings_class_id")]
[Index("Id", Name = "idx_record_mappings_id")]
[Index("ProjectId", Name = "idx_record_mappings_project_id")]
[Index("TagId", Name = "idx_record_mappings_tag_id")]
public partial class RecordMapping
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("record_params", TypeName = "jsonb")]
    public string RecordParams { get; set; } = null!;

    [Column("class_id")]
    public long? ClassId { get; set; }

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

    [Column("project_id")]
    public long ProjectId { get; set; }

    [Column("tag_id")]
    public long? TagId { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("RecordMappings")]
    public virtual Class? Class { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("RecordMappings")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("TagId")]
    [InverseProperty("RecordMappings")]
    public virtual Tag? Tag { get; set; }
}
