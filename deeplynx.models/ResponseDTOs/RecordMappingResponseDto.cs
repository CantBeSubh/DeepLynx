using System.Text.Json.Nodes;

namespace deeplynx.models;

public class RecordMappingResponseDto
{
    public long Id { get; set; }
    public JsonObject? RecordParams { get; set; }
    public long? ClassId { get; set; }
    public long ProjectId { get; set; }
    public long DataSourceId { get; set; }
    public long? TagId { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
    public bool IsArchived { get; set; } = false;
}