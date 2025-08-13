namespace deeplynx.models;

public class HistoricalRecordResponseDto
{
    public long? Id { get; set; }
    public string? Uri { get; set; }
    public string Properties { get; set; } = null!;
    public string? OriginalId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public long? ClassId { get; set; }
    public string? ClassName { get; set; }
    public long? MappingId { get; set; }
    public long DataSourceId { get; set; }
    public string DataSourceName { get; set; }
    public long ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Tags { get; set; } = null!;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}