using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

[Table("records", Schema = "deeplynx")]
[Index("ClassId", Name = "idx_records_class_id")]
[Index("DataSourceId", Name = "idx_records_data_source_id")]
[Index("Id", Name = "idx_records_id")]
[Index("Name", Name = "idx_records_name")]
[Index("OriginalId", Name = "idx_records_original_id")]
[Index("ProjectId", Name = "idx_records_project_id")]
[Index("MappingId", Name="idx_records_mapping_id")]
[Index(nameof(ProjectId), nameof(DataSourceId), nameof(OriginalId), IsUnique = true, Name = "unique_record_original_id")]
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
    
    [Column("description")]
    public string Description { get; set; } = null!;
    
    [Column("class_id")]
    public long? ClassId { get; set; }
    
    [Column("mapping_id")]
    public long? MappingId { get; set; }

    [Column("data_source_id")]
    public long DataSourceId { get; set; }
    
    [Column("object_storage_id")]
    public long? ObjectStorageId { get; set; }

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

    [ForeignKey("ClassId")]
    [InverseProperty("Records")]
    public virtual Class? Class { get; set; }
    
    [ForeignKey("MappingId")]
    [InverseProperty("Records")]
    public virtual RecordMapping? RecordMapping { get; set; }

    [ForeignKey("DataSourceId")]
    [InverseProperty("Records")]
    public virtual DataSource DataSource { get; set; } = null!;
    
    [ForeignKey("ObjectStorageId")]
    [InverseProperty("Records")]
    public virtual ObjectStorage ObjectStorage { get; set; } = null!;

    [InverseProperty("Destination")]
    public virtual ICollection<Edge> EdgeDestinations { get; set; } = new List<Edge>();

    [InverseProperty("Origin")]
    public virtual ICollection<Edge> EdgeOrigins { get; set; } = new List<Edge>();

    [ForeignKey("ProjectId")]
    [InverseProperty("Records")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("RecordId")]
    [InverseProperty("Records")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    
    [InverseProperty("Record")]
    public virtual ICollection<HistoricalRecord> HistoricalRecords { get; set; } = new List<HistoricalRecord>();
}
