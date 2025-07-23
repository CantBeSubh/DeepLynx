using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace deeplynx.models;

public class RecordRequestDto : AbstractNexusCoreDataRequestDto
{
    
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
    
    [Required]
    [JsonPropertyName("properties")]
    public JsonObject Properties { get; set; }
    
    [JsonPropertyName("original_id")]
    public string? OriginalId { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("class_id")]
    public long? ClassId { get; set; }
    
    [JsonPropertyName("class_name")]
    public string? ClassName { get; set; }
    
}