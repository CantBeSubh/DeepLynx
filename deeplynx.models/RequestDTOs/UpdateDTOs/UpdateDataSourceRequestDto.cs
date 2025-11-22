using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class UpdateDataSourceRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Abbreviation { get; set; }
    public string? Type { get; set; }
    public string? BaseUri { get; set; }
    public JsonObject? Config { get; set; }
    [JsonPropertyName("default")] public bool Default { get; set; } = false;
}