using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

// Note that foreign keys other than record_id are absent- this is because for historical records,
// foreign keys are declarative and informative instead of referential in nature.

[Table("historical_records", Schema = "deeplynx")]
[Index("Id", Name = "idx_historical_records_id")]
[Index("RecordId", Name = "idx_historical_records_record_id")]
[Index("ClassName", Name = "idx_historical_records_class_name")]
[Index("LastUpdatedAt", Name = "idx_historical_records_last_updated_at")]
public partial class HistoricalRecord
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("record_id")]
    public long RecordId { get; set; }

    [Column("uri")]
    public string? Uri { get; set; }
    
    [Column("name")]
    public string? Name { get; set; }
    
    [Column("description")]
    public string? Description { get; set; }

    [Column("properties", TypeName = "jsonb")]
    public string Properties { get; set; } = null!;

    [Column("original_id")]
    public string? OriginalId { get; set; }

    [Column("class_id")]
    public long? ClassId { get; set; }
    
    [Column("class_name")]
    public string? ClassName { get; set; }
    
    [Column("mapping_id")]
    public long? MappingId { get; set; }

    [Column("data_source_id")]
    public long DataSourceId { get; set; }
    
    [Column("data_source_name")]
    public string DataSourceName { get; set; }
    
    [Column("object_storage_id")]
    public long? ObjectStorageId { get; set; }
    
    [Column("object_storage_name")]
    public string? ObjectStorageName { get; set; }

    [Column("project_id")]
    public long ProjectId { get; set; }
    
    [Column("project_name")]
    public string ProjectName { get; set; }
    
    [Column("tags", TypeName = "jsonb")]
    public string? Tags { get; set; }

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
    
    [Column("last_updated_at", TypeName = "timestamp without time zone")]
    public DateTime LastUpdatedAt { get; set; }
    
    [ForeignKey("RecordId")]
    [InverseProperty("HistoricalRecords")]
    public virtual Record Record { get; set; } = null!;
}
