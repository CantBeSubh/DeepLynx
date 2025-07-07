namespace deeplynx.models;
using System.Text.Json.Nodes;

public class RecordResponseDto
{
    public long Id { get; set; }
    public string? Uri { get; set; }
    public string Properties { get; set; } = null!;
    public string? OriginalId { get; set; }
    public string? Name { get; set; }
    public long? ClassId { get; set; }
    public long DataSourceId { get; set; }
    public long ProjectId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
}