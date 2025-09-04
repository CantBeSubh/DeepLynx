using System.Text.Json.Nodes;

namespace deeplynx.models;

public class DataSourceResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool Default { get; set; }
    public string? Abbreviation { get; set; }
    public string? Type { get; set; }
    public string? BaseUri { get; set; }
    public JsonObject? Config { get; set; }
    public long ProjectId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
}