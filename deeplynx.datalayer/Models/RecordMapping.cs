using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("record_mappings", Schema = "deeplynx")]
[Index("ClassId", Name = "idx_record_mappings_class_id")]
[Index("DataSourceId", Name = "idx_record_mappings_data_source_id")]
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

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("last_updated_by")]
    public string? LastUpdatedBy { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }

    [Column("tag_id")]
    public long? TagId { get; set; }

    [Column("data_source_id")]
    public long DataSourceId { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("RecordMappings")]
    public virtual Class? Class { get; set; }

    [ForeignKey("DataSourceId")]
    [InverseProperty("RecordMappings")]
    public virtual DataSource DataSource { get; set; } = null!;

    [ForeignKey("ProjectId")]
    [InverseProperty("RecordMappings")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("RecordMapping")]
    public virtual ICollection<RecordMappingTag> RecordMappingTags { get; set; } = new List<RecordMappingTag>();

    [InverseProperty("Mapping")]
    public virtual ICollection<Record> Records { get; set; } = new List<Record>();
}
