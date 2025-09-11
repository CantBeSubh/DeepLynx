using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("records", Schema = "deeplynx")]
[Index("ObjectStorageId", Name = "IX_records_object_storage_id")]
[Index("ClassId", Name = "idx_records_class_id")]
[Index("DataSourceId", Name = "idx_records_data_source_id")]
[Index("Id", Name = "idx_records_id")]
[Index("Name", Name = "idx_records_name")]
[Index("OriginalId", Name = "idx_records_original_id")]
[Index("ProjectId", Name = "idx_records_project_id")]
[Index("ProjectId", "DataSourceId", "OriginalId", Name = "unique_record_original_id", IsUnique = true)]
public partial class Record
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uri")]
    public string? Uri { get; set; }

    [Column("properties", TypeName = "jsonb")]
    public string Properties { get; set; } = null!;

    [Column("original_id")]
    public string OriginalId { get; set; } = null!;

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("class_id")]
    public long? ClassId { get; set; }

    [Column("data_source_id")]
    public long DataSourceId { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }

    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }

    [Column("last_updated_by")]
    public string? LastUpdatedBy { get; set; }

    [Column("description")]
    public string Description { get; set; } = null!;

    [Column("object_storage_id")]
    public long? ObjectStorageId { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("Records")]
    public virtual Class? Class { get; set; }

    [ForeignKey("DataSourceId")]
    [InverseProperty("Records")]
    public virtual DataSource DataSource { get; set; } = null!;

    [InverseProperty("Destination")]
    public virtual ICollection<Edge> EdgeDestinations { get; set; } = new List<Edge>();

    [InverseProperty("Origin")]
    public virtual ICollection<Edge> EdgeOrigins { get; set; } = new List<Edge>();

    [InverseProperty("Record")]
    public virtual ICollection<HistoricalRecord> HistoricalRecords { get; set; } = new List<HistoricalRecord>();

    [ForeignKey("ObjectStorageId")]
    [InverseProperty("Records")]
    public virtual ObjectStorage? ObjectStorage { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Records")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("RecordId")]
    [InverseProperty("Records")]
    public virtual ICollection<SensitivityLabel> Labels { get; set; } = new List<SensitivityLabel>();

    [ForeignKey("RecordId")]
    [InverseProperty("Records")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
