using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class RecordTagDto
{
    public long Id { get; set; }
    public string Name { get; set; }
}

public class RecordResponseDto
{
    [Column("id")]
    public long Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("description")]
    public string Description { get; set; }
    [Column("uri")]
    public string? Uri { get; set; }
    [Column("properties")]
    public string Properties { get; set; } = null!;
    [Column("original_id")]
    public string? OriginalId { get; set; }
    [Column("class_id")]
    public long? ClassId { get; set; }
    [Column("data_source_id")]
    public long DataSourceId { get; set; }
    [Column("project_id")]
    public long ProjectId { get; set; }
    [Column("created_by")]
    public string? CreatedBy { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }
    [Column("archived_at")]
    public DateTime? ArchivedAt { get; set; }
    
    [NotMapped]
    public ICollection<RecordTagDto> Tags { get; set; }
}