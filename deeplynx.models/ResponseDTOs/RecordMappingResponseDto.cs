using System.Text.Json.Nodes;

namespace deeplynx.models;

public class RecordMappingResponseDto
{
    public long Id { get; set; }
    public JsonObject? RecordParams { get; set; }
    public long? ClassId { get; set; }
    public long ProjectId { get; set; }
    public long? TagId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}